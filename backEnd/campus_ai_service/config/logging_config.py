import logging
import sys
from config.settings import BASE_DIR

def setup_logging(log_file="api.log"):
    logger = logging.getLogger()
    logger.setLevel(logging.WARNING)
    logger.handlers.clear()

    # 关键修复：不强制要求 request_id
    formatter = logging.Formatter(
        "%(asctime)s - %(levelname)s - %(message)s"
    )

    file_handler = logging.FileHandler(
        BASE_DIR / log_file, encoding="utf-8"
    )
    file_handler.setFormatter(formatter)
    logger.addHandler(file_handler)

    # 屏蔽 asyncio 多余错误
    logging.getLogger('asyncio').setLevel(logging.CRITICAL)