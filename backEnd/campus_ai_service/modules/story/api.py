# 统一导出所有接口，给 main.py 使用
from .theme import get_theme_info, force_refresh_theme
from .generator import story_start, story_continue
from .state import reset_user_story

# 重置故事
def reset_story(user_id: str):
    reset_user_story(user_id)
    return {"code": 200, "msg": "故事已重置"}