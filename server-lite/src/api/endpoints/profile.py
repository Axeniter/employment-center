from fastapi import APIRouter, Depends, HTTPException, status
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
from core.utils import save_upload_file, delete_file
from config import config

profile_router = APIRouter(prefix="/profile", tags=["profile"])

@profile_router.get("/me", response_model=Union[ApplicantProfileResponse, EmployerProfileResponse])
async def get_my_profile(user = Depends(get_current_active_user), db = Depends(get_db)):
    profile = await get_profile_by_id(db, user.id)
    if not user.profile:
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