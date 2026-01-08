from pydantic import BaseModel, EmailStr
from typing import Optional
from model.user import UserRole
from uuid import UUID

class Token(BaseModel):
    access_token: str
    refresh_token: str

class TokenData(BaseModel):
    user_id: UUID
    email: EmailStr
    role: UserRole

class RefreshTokenRequest(BaseModel):
    refresh_token: str