import json
import logging
from core.ai_client import call_ai
from knowledge.excel_kb import campus_kb
from prompts.elf_prompt import ELF_QA_PROMPT, NPC_PROMPTS
from config.settings import LOCAL_MODE
from modules.elf.memory_manager import get_user_memory

logger = logging.getLogger(__name__)

def get_answer(question: str, player_id: str = None, category: str = "default"):
    # 1. 【严格隔离】只查当前NPC的知识
    knowledge = campus_kb.search(question, category=category)
    target_npc = None

    # 2. 无知识 → 路由到正确NPC
    if not knowledge and category not in ["default", "echo"]:
        target_npc = campus_kb.route_to_best_npc(question)

    # 3. 【独立记忆】只加载当前NPC的对话
    memory = get_user_memory(player_id, category) if player_id else None
    history = memory.load_memory_variables({}) if memory else "无历史对话"

    if LOCAL_MODE:
        ans = knowledge[:100] + "..." if knowledge else "暂无相关信息哦～"
        if target_npc:
            ans = f"这个问题你可以问问【{campus_kb.npc_map.get(target_npc)}】哦~"
        return json.dumps({"answer": ans, "category": category}, ensure_ascii=False)

    # 4. 加载NPC人设（绝对不会混）
    system_prompt = NPC_PROMPTS.get(category, ELF_QA_PROMPT)
    knowledge = knowledge.replace('\n', ' ').strip()

    # 5. 构造对话
    messages = [
        {"role": "system", "content": system_prompt},
        {"role": "user", "content": f"历史对话：\n{history}\n\n问题：{question}\n知识库：{knowledge}"}
    ]

    ai_content = call_ai(messages)

    # 6. 异常兜底（严格人设 + 推荐）
    if not ai_content:
        ans = "暂无相关信息哦～"
        if target_npc:
            ans = f"这个问题你可以问问【{campus_kb.npc_map.get(target_npc)}】哦~"
        return json.dumps({"answer": ans, "category": category}, ensure_ascii=False)

    # 7. 保存记忆（只保存在当前NPC）
    if player_id and memory:
        memory.save_context({"input": question}, {"output": ai_content})

    # 8. 返回结果
    try:
        res = json.loads(ai_content)
        return json.dumps(res, ensure_ascii=False)
    except:
        ans = knowledge if knowledge else "暂无相关信息哦～"
        if target_npc:
            ans = f"这个问题你可以问问【{campus_kb.npc_map.get(target_npc)}】哦~"
        return json.dumps({"answer": ans, "category": category}, ensure_ascii=False)