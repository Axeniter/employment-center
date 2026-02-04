from fastapi import APIRouter
from .endpoints import (auth, profile, vacancy, event, chat)

api_router = APIRouter(prefix="/api")

api_router.include_router(auth.auth_router)
api_router.include_router(profile.profile_router)
api_router.include_router(vacancy.vacancy_router)
api_router.include_router(vacancy.response_router)
api_router.include_router(event.event_router)
api_router.include_router(chat.chat_router)
