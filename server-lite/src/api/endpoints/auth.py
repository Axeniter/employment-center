from fastapi import APIRouter, Depends, HTTPException, status
from schemas.user import UserResponse, UserCreate, UserLogin
from schemas.auth import Token, RefreshTokenRequest
from sqlalchemy.ext.asyncio import AsyncSession
from core.database import get_db
from core.security import (create_access_token, create_and_save_refresh_token, get_user_id_by_refresh_token,
                           delete_refresh_token)
from orm.user import create_user, get_user_by_email, authenticate_user, get_user_by_id

auth_router = APIRouter(prefix="/auth", tags=["auth"])

@auth_router.post("/register", response_model=UserResponse)
async def register(user_data: UserCreate, db: AsyncSession = Depends(get_db)):
    user = await get_user_by_email(db, user_data.email)
    if user:
        raise HTTPException(status_code=status.HTTP_400_BAD_REQUEST,
                            detail="User alreay exists")
    user = await create_user(db, user_data)
    return user

@auth_router.post("/login", response_model=Token)
async def login(user_data: UserLogin, db: AsyncSession = Depends(get_db)):
    user = await authenticate_user(db, user_data.email, user_data.password)
    if not user:
        raise HTTPException(status_code=status.HTTP_401_UNAUTHORIZED,
                            detail="Incorrect email or password",
                            headers={"WWW-Authenticate": "Bearer"})
    
    access_token = create_access_token({"email": user.email,
                                        "user_id": str(user.id),
                                        "role": user.role})
    refresh_token = await create_and_save_refresh_token(user.id)

    return Token(access_token=access_token, refresh_token=refresh_token)

@auth_router.post("/refresh", response_model=Token)
async def refresh_token(refresh_data: RefreshTokenRequest, db: AsyncSession = Depends(get_db)):
    user_id = await get_user_id_by_refresh_token(refresh_data.refresh_token)
    if not user_id:
        raise HTTPException(status_code=status.HTTP_401_UNAUTHORIZED, detail="Invalid refresh token")
    
    user = await get_user_by_id(db, user_id)
    if not user:
        raise HTTPException(status_code=status.HTTP_401_UNAUTHORIZED, detail="User not found")
    
    access_token = create_access_token({"email": user.email,
                                        "user_id": str(user.id),
                                        "role": user.role})
    await delete_refresh_token(refresh_data.refresh_token)
    refresh_token = await create_and_save_refresh_token(user.id)

    return Token(access_token=access_token, refresh_token=refresh_token)
