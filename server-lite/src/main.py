import sys
import os

sys.path.append(os.path.join(os.path.dirname(__file__), "..", "..", "src"))

from fastapi import FastAPI
from fastapi.middleware.cors import CORSMiddleware
from fastapi.staticfiles import StaticFiles
from config import config
from core.database import Base, engine
from api.api import api_router
from contextlib import asynccontextmanager

os.makedirs(config.UPLOAD_DIR, exist_ok=True)

async def init_db():
    async with engine.begin() as conn:
        await conn.run_sync(Base.metadata.create_all)

@asynccontextmanager
async def lifespan(app: FastAPI):
    await init_db()
    yield

app = FastAPI(title="Workich", lifespan=lifespan)

app.add_middleware(
    CORSMiddleware,
    allow_origins=config.ALLOWED_ORIGINS,
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

app.mount("/uploads", StaticFiles(directory="uploads"), name="uploads")

app.include_router(api_router)

print("жопа")

@app.get("/")
async def root():
    return {"name": "Workich", "status": "healthy"}
