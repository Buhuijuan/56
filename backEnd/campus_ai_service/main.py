from fastapi import FastAPI, HTTPException, Request, Depends
from fastapi.middleware.cors import CORSMiddleware
from fastapi.middleware.gzip import GZipMiddleware
from fastapi.responses import JSONResponse
import uvicorn
import json
import uuid
import time
import sys
import asyncio
import multiprocessing
import os
import logging
import uuid
from config.logging_config import setup_logging
from config.settings import SCENE_CONFIG_PATH, LOCAL_MODE
from core.rate_limit import rate_limit_middleware

# 修复 Windows asyncio 问题
if sys.platform == 'win32':
    asyncio.set_event_loop_policy(asyncio.WindowsSelectorEventLoopPolicy())

# 日志初始化（已修复）
setup_logging()

# FastAPI
app = FastAPI(title="校园AI后端", version="2.0")

# 跨域
app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)
app.add_middleware(GZipMiddleware, minimum_size=100)

# 请求ID中间件（稳定版）
@app.middleware("http")
async def req_middleware(request: Request, call_next):
    request.state.rid = str(uuid.uuid4())[:8]
    response = await call_next(request)
    response.headers["X-Request-ID"] = request.state.rid
    return response

# 场景解析
def extract_scene(question):
    mapping = {
        "图书馆": ["图书馆", "借书"],
        "一食堂": ["一食堂"],
        "二食堂": ["二食堂"],
        "缙湖": ["缙湖", "钓鱼"],
        "钟楼": ["钟楼"],
        "风雨操场": ["风雨操场"],
    }
    for scene, kws in mapping.items():
        if any(kw in question for kw in kws):
            return scene
    return None

# 场景坐标
class SceneManager:
    def __init__(self, path):
        self.path = path
        self.data = {}
        self.load()

    def load(self):
        if not os.path.exists(self.path):
            self.data = {
                "图书馆": {"x": 100, "y": 0, "z": 50},
                "一食堂": {"x": 50, "y": 0, "z": 80}
            }
            with open(self.path, "w", encoding="utf-8") as f:
                json.dump(self.data, f, ensure_ascii=False, indent=2)
        else:
            with open(self.path, encoding="utf-8") as f:
                self.data = json.load(f)

    def pos(self, name):
        return self.data.get(name, {"x": 0, "y": 0, "z": 0})

scene_mgr = SceneManager(SCENE_CONFIG_PATH)

# ------------------------------
# API 接口
@app.post("/api/campus/qa")
async def qa(
    req: Request,
    question: str,
    player_id: str = None,
    category: str = "default",  # 新增：前端传入NPC分类
    scene_name: str = None,
    ip=Depends(rate_limit_middleware)
):
    from modules.elf.qa_service import get_answer
    target = extract_scene(question)
    # 传递 category 给服务层
    ans_str = await asyncio.to_thread(get_answer, question, player_id, category)
    ans = json.loads(ans_str)

    return {
        "code": 200,
        "msg": "success",
        "request_id": req.state.rid,
        "data": {
            "answer": ans.get("answer", "暂无信息"),
            "category": category,
            "target_scene": target,
            "target_position": scene_mgr.pos(target)
        }
    }

# ====================== 故事接龙接口（直接从新模块导入） ======================
@app.get("/api/story/theme/info", summary="获取故事主题+剩余时间+开头")
async def get_story_theme_info():
    from modules.story.api import get_theme_info
    return get_theme_info()

@app.post("/api/story/start", summary="开始故事（传入昵称）")
async def story_start(user_id: str, nickname: str):
    if not user_id or not nickname:
        raise HTTPException(400, "user_id 和 nickname 不能为空")
    from modules.story.api import story_start
    return story_start(user_id, nickname)

@app.post("/api/story/next", summary="故事续写")
async def story_next(user_id: str, choice: str = None, custom_choice: str = None):
    if not user_id:
        raise HTTPException(400, "user_id 必填")
    from modules.story.api import story_continue
    return story_continue(user_id, choice, custom_choice)

@app.post("/api/story/reset", summary="重置故事")
async def story_reset(user_id: str):
    if not user_id:
        raise HTTPException(400, "user_id 必填")
    from modules.story.api import reset_story
    return reset_story(user_id)

@app.post("/api/story/force_update_theme", summary="强制更新主题")
async def force_update_theme():
    from modules.story.api import force_refresh_theme
    return force_refresh_theme()

@app.get("/api/health")
async def health():
    return {"status": "healthy"}

# ------------------------------
# 启动 HTTP / HTTPS
# ------------------------------
def run_http():
    os.environ["TOKENIZERS_PARALLELISM"] = "false"

    from knowledge.excel_kb import campus_kb
    from modules.elf.qa_service import get_answer
    print("✅ 模型在子进程加载完成!")

    try:
        uvicorn.run(
            "main:app",
            host="0.0.0.0",
            port=8000,
            log_level="info",
            access_log=True
        )
    except Exception as e:
        print(f"HTTP启动失败: {e}")

def run_https():
    return

# ------------------------------
# 主函数：不加载任何模型
# ------------------------------
def main():
    if sys.platform == 'win32':
        multiprocessing.freeze_support()

    p1 = multiprocessing.Process(target=run_http, daemon=True)
    p2 = multiprocessing.Process(target=run_https, daemon=True)

    p1.start()
    p2.start()
    print("✅ HTTP  运行在 :8000")
    print("✅ HTTPS 已关闭")
    p1.join()

if __name__ == "__main__":
    main()