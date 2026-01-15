from fastapi import APIRouter
from endpoints import (auth, profile)

api_router = APIRouter(prefix="/api")

api_router.include_router(auth.auth_router)
api_router.include_router(profile.profile_router)
