import sys
import os

sys.path.append(os.path.join(os.path.dirname(__file__), "..", "..", "src"))

from fastapi import FastAPI
from fastapi.middleware.cors import CORSMiddleware
from fastapi.staticfiles import StaticFiles
from config import config
from core.database import Base, engine
from api.api import api_router

os.makedirs(config.UPLOAD_DIR, exist_ok=True)

Base.metadata.create_all(bind=engine)

app = FastAPI(title="Workich")

app.add_middleware(
    CORSMiddleware,
    allow_origins=config.ALLOWED_ORIGINS,
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

app.mount("/uploads", StaticFiles(directory="uploads"), name="uploads")

app.include_router(api_router)

@app.get("/")
async def root():
    return {"name": "Workich", "status": "healthy"}
