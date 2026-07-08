import os
import sys
import json
import base64
import dashscope
from dashscope import Generation
from cryptography.fernet import Fernet
from config.settings import BASE_DIR, ENCRYPTED_API_KEY_B64, SECRET_KEY_FILENAME, LOCAL_MODE
import logging

logger = logging.getLogger(__name__)

def get_program_root_path():
    if getattr(sys, "frozen", False):
        return os.path.dirname(sys.executable)
    else:
        return os.path.abspath(BASE_DIR)

def decrypt_api_key():
    try:
        root = get_program_root_path()
        paths = [
            os.path.join(root, SECRET_KEY_FILENAME),
            os.path.join(os.getcwd(), SECRET_KEY_FILENAME),
            os.path.join(os.path.dirname(root), SECRET_KEY_FILENAME),
        ]
        key_path = None
        for p in paths:
            if os.path.isfile(p):
                key_path = p
                break
        if not key_path:
            raise Exception("未找到secret.key")

        with open(key_path, "rb") as f:
            key = f.read()

        fernet = Fernet(key)
        encrypted = base64.b64decode(ENCRYPTED_API_KEY_B64)
        return fernet.decrypt(encrypted).decode()
    except Exception as e:
        logger.error(f"解密失败: {e}")
        raise Exception("AI服务初始化失败")

def init_api():
    if LOCAL_MODE:
        return
    api_key = decrypt_api_key()
    dashscope.api_key = api_key

def call_ai(messages, is_story_mode=False):
    if LOCAL_MODE:
        return ""

    try:
        init_api()
        response = Generation.call(
            model="qwen-plus",
            messages=messages,
            result_format="message",
            temperature=0.9,
            max_tokens=1024,
            repetition_penalty=1.05,
            seed=1314
        )
        if response.status_code == 200:
            return response.output.choices[0].message.content.strip()
        else:
            logger.error(f"AI接口错误: {response.message}")
            return None
    except Exception as e:
        logger.error(f"AI调用异常: {e}")
        return None