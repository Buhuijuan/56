import time
from fastapi import Request, HTTPException
from config.settings import RATE_LIMIT, RATE_LIMIT_WINDOW

ip_requests = {}

async def rate_limit_middleware(request: Request):
    client_ip = request.client.host
    now = time.time()

    if client_ip in ip_requests:
        ip_requests[client_ip] = [
            t for t in ip_requests[client_ip] if now - t < RATE_LIMIT_WINDOW
        ]
        if len(ip_requests[client_ip]) >= RATE_LIMIT:
            raise HTTPException(status_code=429, detail="请求过于频繁")
    else:
        ip_requests[client_ip] = []

    ip_requests[client_ip].append(now)
    return client_ip