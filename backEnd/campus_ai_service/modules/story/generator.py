import json
import logging
import random
from core.ai_client import call_ai
from knowledge.excel_kb import campus_kb
from prompts.elf_prompt import (
    ELF_STORY_SYSTEM,
    STORY_START_PROMPT,
    STORY_CONTINUE_PROMPT,
    STORY_END_PROMPT
)
from .config import STORY_CONTENT_LIMIT, OPTION_LIMIT, TOTAL_STORY_ROUNDS
from .state import (
    init_user_state, get_user_state,
    save_user_history, next_user_round, build_history_text
)
from .theme import NPC_ALIAS_MAP

logger = logging.getLogger(__name__)

def story_start(user_id: str, nickname: str):
    state = init_user_state(user_id, nickname)
    theme = state["theme"]
    npc_cat = state["npc_category"]
    npc_alias = NPC_ALIAS_MAP.get(npc_cat, "校园NPC")

    # ✅ 检索该 NPC 全部相关知识（用于创作）
    knowledge = campus_kb.search("", category=npc_cat)

    # ✅ 加入随机种子，确保每次生成不一样
    rand_seed = random.randint(1000, 9999)

    prompt = f"""
你是校园故事作家，用温暖、拟人、自然的语言写故事。
用户昵称：{nickname}
主题：{theme}
NPC 角色：{npc_alias}
知识库内容：{knowledge}
要求：
- 基于知识 paraphrase 创作，不照搬原句
- 贴近真实大学生活
- 有人情味
- 段落≤150字
- 选项3个，每个≤17字
- 每次生成风格不同（随机种子：{rand_seed}）
返回JSON：{{"content":"...","options":["...","...","..."]}}
"""
    messages = [
        {"role": "system", "content": ELF_STORY_SYSTEM},
        {"role": "user", "content": prompt}
    ]

    try:
        data = json.loads(call_ai(messages))
        save_user_history(user_id, data["content"], "开始故事")
        return {
            "code": 200,
            "theme": theme,
            "content": data["content"],
            "options": data["options"],
            "round": 1,
            "is_end": False
        }
    except Exception as e:
        logger.error(f"故事开始失败: {e}")
        return {
            "code": 500,
            "theme": theme,
            "content": f"{nickname}，故事开始啦！",
            "options": ["继续探索", "校园冒险", "寻找伙伴"],
            "round": 1,
            "is_end": False
        }

# story_continue 逻辑保持不变（已正确）
def story_continue(user_id: str, choice: str = None, custom_choice: str = None):
    state = get_user_state(user_id)
    if not state:
        return {"code": 400, "msg": "请先开始故事"}

    final_choice = custom_choice.strip() if custom_choice else choice
    if not final_choice:
        return {"code": 400, "msg": "请选择或输入故事走向"}

    current_round = state["current_round"]
    if current_round >= TOTAL_STORY_ROUNDS:
        return {"code": 400, "msg": "故事已结束，请重置"}

    nickname = state["nickname"]
    theme = state["theme"]
    npc_cat = state["npc_category"]
    npc_alias = NPC_ALIAS_MAP.get(npc_cat, "校园NPC")
    knowledge = campus_kb.search("", category=npc_cat)
    history_text = build_history_text(state["history"])
    next_round = current_round + 1
    is_end = (next_round == TOTAL_STORY_ROUNDS)

    if is_end:
        prompt = STORY_END_PROMPT.format(
            nickname=nickname, theme=theme, knowledge=knowledge,
            history=history_text, choice=final_choice, content_limit=STORY_CONTENT_LIMIT
        )
    else:
        prompt = STORY_CONTINUE_PROMPT.format(
            nickname=nickname, theme=theme, knowledge=knowledge,
            history=history_text, choice=final_choice,
            content_limit=STORY_CONTENT_LIMIT, option_limit=OPTION_LIMIT
        )

    messages = [{"role": "system", "content": ELF_STORY_SYSTEM}, {"role": "user", "content": prompt}]

    try:
        data = json.loads(call_ai(messages))
        save_user_history(user_id, data["content"], final_choice)
        next_user_round(user_id)
        return {
            "code": 200,
            "theme": theme,
            "content": data["content"],
            "options": data.get("options", []) if not is_end else [],
            "round": next_round,
            "is_end": is_end
        }
    except Exception as e:
        logger.error(f"故事续写失败: {e}")
        return {
            "code": 200,
            "theme": theme,
            "content": f"{nickname}选择了{final_choice}，故事继续...",
            "options": [] if is_end else ["继续前行", "触发剧情", "探索未知"],
            "round": next_round,
            "is_end": is_end
        }