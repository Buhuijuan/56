import time
from .config import TOTAL_STORY_ROUNDS
from .theme import refresh_global_theme

# 用户状态存储
USER_STORY_STATE = {}

def init_user_state(user_id: str, nickname: str):
    """初始化用户故事状态"""
    theme_data = refresh_global_theme()
    USER_STORY_STATE[user_id] = {
        "nickname": nickname,
        "current_round": 1,
        "history": [],
        "theme": theme_data["current_theme"],
        "npc_category": theme_data["npc_category"],
        "update_time": time.time()
    }
    return USER_STORY_STATE[user_id]

def get_user_state(user_id: str):
    """获取当前用户状态（支持续写）"""
    return USER_STORY_STATE.get(user_id, None)

def save_user_history(user_id: str, content: str, choice: str):
    """保存故事历史"""
    state = get_user_state(user_id)
    if not state:
        return
    state["history"].append({
        "round": state["current_round"],
        "content": content,
        "choice": choice
    })
    # 保留最多4轮
    if len(state["history"]) > TOTAL_STORY_ROUNDS:
        state["history"].pop(0)

def next_user_round(user_id: str):
    """轮数+1"""
    state = get_user_state(user_id)
    if state:
        state["current_round"] += 1
        return state["current_round"]
    return 1

def reset_user_story(user_id: str):
    """重置故事"""
    if user_id in USER_STORY_STATE:
        del USER_STORY_STATE[user_id]

def build_history_text(history: list):
    """格式化历史剧情"""
    if not history:
        return "无历史剧情"
    text = ""
    for item in history:
        text += f"第{item['round']}轮：{item['content']} 选择：{item['choice']}\n"
    return text