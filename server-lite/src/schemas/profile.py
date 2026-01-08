from pydantic import BaseModel
from uuid import UUID
from typing import Optional, List
from datetime import datetime

class ProfileBase(BaseModel):
    avatar_url: Optional[str] = None

class ApplicantProfileCreate(ProfileBase):
    first_name: str
    last_name: str
    middle_name: Optional[str] = None
    phone_number: Optional[str] = None
    birth_date: Optional[datetime] = None
    city: Optional[str] = None
    about:  Optional[str] = None
    skills: List[str] = []

class ApplicantProfileUpdate(ProfileBase):
    first_name: Optional[str] = None
    last_name: Optional[str] = None
    middle_name: Optional[str] = None
    phone_number: Optional[str] = None
    birth_date: Optional[datetime] = None
    city: Optional[str] = None
    about:  Optional[str] = None
    skills: Optional[List[str]] = None

class EmployerProfileCreate(ProfileBase):
    company_name: str
    description: Optional[str]
    contact: Optional[str]

class EmployerProfileUpdate(ProfileBase):
    company_name: Optional[str] = None
    description: Optional[str] = None
    contact: Optional[str] = None

class ApplicantProfileResponse(ProfileBase):
    user_id: UUID
    first_name: str
    last_name: str
    middle_name: str
    phone_number: str
    birth_date: datetime
    city: str
    about: str
    skills: List[str]

    class Config:
        from_attributes = True

class EmployerProfileResponse(ProfileBase):
    user_id: UUID
    company_name: str
    description: str
    contact: str

    class Config:
        from_attributes = True