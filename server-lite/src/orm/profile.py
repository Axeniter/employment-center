from sqlalchemy.ext.asyncio import AsyncSession
from sqlalchemy import select
from models.profile import ApplicantProfile, EmployerProfile
from schemas.profile import ApplicantProfileCreate, ApplicantProfileUpdate, EmployerProfileCreate, EmployerProfileUpdate
from models.user import User, UserRole
from uuid import UUID

async def get_profile_by_id(db: AsyncSession, user_id: UUID):
    user_result = await db.execute(
        select(User).where(User.id == user_id)
    )
    user = user_result.scalar_one_or_none()
    
    if not user:
        return None
    
    if user.role == UserRole.APPLICANT:
        result = await db.execute(
            select(ApplicantProfile)
            .where(ApplicantProfile.user_id == user_id)
        )
        return result.scalar_one_or_none()
    else:
        result = await db.execute(
            select(EmployerProfile)
            .where(EmployerProfile.user_id == user_id)
        )
        return result.scalar_one_or_none()


async def create_applicant_profile(db: AsyncSession, user_id: UUID, profile: ApplicantProfileCreate):
    db_profile = ApplicantProfile(
        user_id=user_id,
        **profile.model_dump()
    )
    db.add(db_profile)
    await db.commit()
    await db.refresh(db_profile)
    return db_profile


async def create_employer_profile(db: AsyncSession, user_id: UUID, profile: EmployerProfileCreate):
    db_profile = EmployerProfile(
        user_id=user_id,
        **profile.model_dump()
    )
    db.add(db_profile)
    await db.commit()
    await db.refresh(db_profile)
    return db_profile

async def update_applicant_profile(db: AsyncSession, user_id: UUID, profile: ApplicantProfileUpdate):
    db_profile = await get_profile_by_id(db, user_id)
    if not db_profile:
        return None
    
    if not isinstance(db_profile, ApplicantProfile):
        raise ValueError("User does not have an applicant profile")
    
    update_data = profile.model_dump(exclude_none=True)
    for field, value in update_data.items():
        setattr(db_profile, field, value)

    await db.commit()
    await db.refresh(db_profile)
    return db_profile

async def update_employer_profile(db: AsyncSession, user_id: UUID, profile: EmployerProfileUpdate):
    db_profile = await get_profile_by_id(db, user_id)
    if not db_profile:
        return None
    
    if not isinstance(db_profile, EmployerProfile):
        raise ValueError("User does not have an employer profile")
    
    update_data = profile.model_dump(exclude_none=True)
    for field, value in update_data.items():
        setattr(db_profile, field, value)

    await db.commit()
    await db.refresh(db_profile)
    return db_profile

async def delete_profile(db: AsyncSession, user_id: UUID):
    db_profile = await get_profile_by_id(db, user_id)
    if not db_profile:
        return None
    
    await db.delete(db_profile)
    await db.commit()
    return True