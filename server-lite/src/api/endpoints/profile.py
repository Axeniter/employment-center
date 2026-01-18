from fastapi import APIRouter, Depends, HTTPException, status, UploadFile
from fastapi.responses import FileResponse
from typing import Union
from schemas.profile import (ApplicantProfileResponse, EmployerProfileResponse, ApplicantProfileCreate, ApplicantProfileUpdate, 
                             EmployerProfileCreate, EmployerProfileUpdate)
from core.dependencies import get_current_active_user, require_role
from core.database import get_db
from orm.profile import (get_profile_by_id, create_employer_profile, create_applicant_profile, 
                         update_applicant_profile, update_employer_profile)
from uuid import UUID
from sqlalchemy.ext.asyncio import AsyncSession
from models.user import UserRole
from core.utils import save_user_avatar, delete_user_avatar, get_user_avatar_path
import os

profile_router = APIRouter(prefix="/profile", tags=["profile"])

@profile_router.get("/me", response_model=Union[ApplicantProfileResponse, EmployerProfileResponse])
async def get_my_profile(user = Depends(get_current_active_user), db = Depends(get_db)):
    profile = await get_profile_by_id(db, user.id)
    if not profile:
        raise HTTPException(status_code=404, detail="Profile doesn't exist")
    
    return profile

@profile_router.get("/{user_id}", response_model=Union[ApplicantProfileResponse, EmployerProfileResponse])
async def get_profile(user_id: UUID, db = Depends(get_db)):
    profile = await get_profile_by_id(db, user_id)
    if not profile:
        HTTPException(status_code=status.HTTP_404_NOT_FOUND, detail="Profile not found")

    return profile

@profile_router.post("/applicant", response_model=ApplicantProfileResponse)
async def create_applicant_profile_endpoint(profile_data: ApplicantProfileCreate, user = Depends(require_role(UserRole.APPLICANT)),
                                    db: AsyncSession = Depends(get_db)):
    profile = await get_profile_by_id(db, user.id)
    if profile:
        raise HTTPException(status_code=status.HTTP_400_BAD_REQUEST,
            detail="Profile already exists")
    
    new_profile = await create_applicant_profile(db, user.id, profile_data)
    return new_profile

@profile_router.post("/employer", response_model=EmployerProfileResponse)
async def create_employer_profile_endpoint(profile_data: EmployerProfileCreate, user = Depends(require_role(UserRole.EMPLOYER)),
                                    db: AsyncSession = Depends(get_db)):
    profile = await get_profile_by_id(db, user.id)
    if profile:
        raise HTTPException(status_code=status.HTTP_400_BAD_REQUEST,
            detail="Profile already exists")
    
    new_profile = await create_employer_profile(db, user.id, profile_data)
    return new_profile

@profile_router.put("/applicant", response_model=ApplicantProfileResponse)
async def update_applicant_profile_endpoint(profile_data: ApplicantProfileUpdate, user = Depends(require_role(UserRole.APPLICANT)),
                                            db: AsyncSession = Depends(get_db)):
    profile = await get_profile_by_id(db, user.id)
    if not profile:
        raise HTTPException(status_code=status.HTTP_400_BAD_REQUEST,
            detail="Profile doesn't exists")
    updated_profile = await update_applicant_profile(db, user.id, profile_data)
    return updated_profile

@profile_router.put("/employer", response_model=EmployerProfileResponse)
async def update_employer_profile_endpoint(profile_data: EmployerProfileUpdate, user = Depends(require_role(UserRole.EMPLOYER)),
                                            db: AsyncSession = Depends(get_db)):
    profile = await get_profile_by_id(db, user.id)
    if not profile:
        raise HTTPException(status_code=status.HTTP_400_BAD_REQUEST,
            detail="Profile doesn't exists")
    updated_profile = await update_employer_profile(db, user.id, profile_data)
    return updated_profile

@profile_router.post("/avatar", status_code=status.HTTP_200_OK)
async def upload_avatar(file: UploadFile, user = Depends(get_current_active_user)):
    try:
        await save_user_avatar(file, user.id)
        return {
            "message": "Avatar uploaded successfully",
            "user_id": str(user.id)
        }
    except HTTPException as e:
        raise e
    except Exception as e:
        raise HTTPException(
            status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
            detail=f"Failed to upload avatar: {str(e)}")
    
@profile_router.delete("/avatar", status_code=status.HTTP_200_OK)
async def remove_avatar(user = Depends(get_current_active_user)):
    try:
        deleted = delete_user_avatar(user.id)
        if deleted:
            return {
                "message": "Avatar deleted successfully",
                "user_id": str(user.id)
            }
        else:
            raise HTTPException(
                status_code=status.HTTP_404_NOT_FOUND,
                detail="Avatar not found")
    except HTTPException as e:
        raise e
    except Exception as e:
        raise HTTPException(
            status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
            detail=f"Failed to delete avatar: {str(e)}"
        )

@profile_router.get("/{user_id}/avatar")
async def get_avatar(user_id: UUID):
    avatar_path = get_user_avatar_path(user_id)
    
    if not avatar_path:
        raise HTTPException(status_code=404, detail="Avatar not found")
    
    extension = os.path.splitext(avatar_path)[1].lower()
    return FileResponse(avatar_path, media_type=f"image/{extension.lstrip('.')}")

@profile_router.get("/me/avatar")
async def get_my_avatar(user = Depends(get_current_active_user)):
    avatar_path = get_user_avatar_path(user.id)
    
    if not avatar_path:
        raise HTTPException(status_code=404, detail="Avatar not found")
    
    extension = os.path.splitext(avatar_path)[1].lower()
    return FileResponse(avatar_path, media_type=f"image/{extension.lstrip('.')}")