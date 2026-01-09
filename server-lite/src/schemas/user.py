from pydantic import BaseModel, EmailStr
from models.user import UserRole
from uuid import UUID

class UserBase(BaseModel):
    email: EmailStr

class UserCreate(UserBase):
    role: UserRole
    password: str

class UserLogin(UserBase):
    password: str
    
class UserResponse(UserBase):
    id: UUID
    role: UserRole

    class Config:
        from_attributes = True

