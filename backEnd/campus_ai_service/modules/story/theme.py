import random
import time
import logging
from datetime import datetime, timedelta
from core.ai_client import call_ai
from knowledge.excel_kb import campus_kb
from prompts.elf_prompt import ELF_STORY_SYSTEM, STORY_THEME_PROMPT
from .config import SEVEN_DAYS_SECONDS, NPC_CATEGORIES

logger = logging.getLogger(__name__)

# 全局主题数据
GLOBAL_STORY_DATA = {
    "current_theme": "",
    "current_opening": "",
    "generate_time": 0,
    "used_themes": [],
    "npc_category": "",
    "npc_alias": ""
}

# ✅ 严格 NPC 别名（你要求的）
NPC_ALIAS_MAP = {
    "volunteer": "迎新志愿者",
    "dorm_manager": "楼管阿姨",
    "nurse": "校医护士",
    "gardener": "园艺工人",
    "librarian": "图书管理员",
    "security": "校园保安",
    "grass_cow": "草地牛",
}

# 只允许这些 NPC 生成故事
ALLOWED_NPC = list(NPC_ALIAS_MAP.keys())

# -------------------------------
# ✅ 每周一 00:00 自动更新（北京时间）
# -------------------------------
def get_monday_next_update():
    now = datetime.now()
    today = now.weekday()  # 0=周一，6=周日
    days_ahead = (7 - today) % 7
    if days_ahead == 0:
        days_ahead = 7
    next_mon = now + timedelta(days=days_ahead)
    next_mon_midnight = next_mon.replace(hour=0, minute=0, second=0, microsecond=0)
    return next_mon_midnight

def get_remaining_until_monday():
    now = datetime.now()
    next_mon = get_monday_next_update()
    delta = next_mon - now
    return str(delta).split('.')[0]  # 格式：1 day, 2:12:34

# -------------------------------
# ✅ 主题生成：严格对应 NPC 知识
# -------------------------------
def generate_theme_from_knowledge():
    selected_category = random.choice(ALLOWED_NPC)
    npc_alias = NPC_ALIAS_MAP[selected_category]
    knowledge = campus_kb.search("", category=selected_category)

    prompt = f"""
你是校园故事生成器，根据【{npc_alias}】的身份和知识库生成4-10字故事主题。
知识库：{knowledge}
要求：
- 必须贴合 {npc_alias} 的工作与校园生活
- 不允许脱离知识库
- 不允许抽象创作
只返回主题文本。
"""
    try:
        theme = call_ai([
            {"role": "system", "content": ELF_STORY_SYSTEM},
            {"role": "user", "content": prompt}
        ]).strip()
        return theme, selected_category, npc_alias
    except:
        return "校园日常小故事", selected_category, npc_alias

def generate_story_opening(theme, npc_category):
    npc_alias = NPC_ALIAS_MAP.get(npc_category, "校园小精灵")
    knowledge = campus_kb.search("", category=npc_category)

    prompt = f"""
生成故事开头前22字，风格温暖自然，贴近大学校园生活。
主题：{theme}
NPC：{npc_alias}
知识库：{knowledge}
只返回文本，不要格式。
"""
    try:
        txt = call_ai([{"role": "user", "content": prompt}]).strip()
        return txt[:22] + "..." if len(txt) > 22 else txt
    except:
        return "阳光洒满校园，故事即将开始..."

# -------------------------------
# ✅ 每周一更新逻辑
# -------------------------------
def refresh_global_theme(force=False):
    now = datetime.now()
    next_mon = get_monday_next_update()

    if force or not GLOBAL_STORY_DATA["current_theme"] or now >= next_mon:
        new_theme, npc_cat, npc_alias = generate_theme_from_knowledge()
        new_opening = generate_story_opening(new_theme, npc_cat)

        GLOBAL_STORY_DATA["current_theme"] = new_theme
        GLOBAL_STORY_DATA["current_opening"] = new_opening
        GLOBAL_STORY_DATA["generate_time"] = time.time()
        GLOBAL_STORY_DATA["npc_category"] = npc_cat
        GLOBAL_STORY_DATA["npc_alias"] = npc_alias

        if new_theme not in GLOBAL_STORY_DATA["used_themes"]:
            GLOBAL_STORY_DATA["used_themes"].append(new_theme)

    return GLOBAL_STORY_DATA

def get_theme_info():
    data = refresh_global_theme()
    return {
        "code": 200,
        "theme": data["current_theme"],
        "opening": data["current_opening"],
        "remaining_time": get_remaining_until_monday()
    }

def force_refresh_theme():
    data = refresh_global_theme(force=True)
    return {"code": 200, "theme": data["current_theme"]}