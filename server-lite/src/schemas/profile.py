from pydantic import BaseModel
from uuid import UUID
from typing import Optional, List, Literal
from datetime import date
from models.user import UserRole

class ApplicantProfileCreate(BaseModel):
    first_name: str
    last_name: str
    middle_name: Optional[str] = None
    phone_number: Optional[str] = None
    birth_date: Optional[date] = None
    city: Optional[str] = None
    about:  Optional[str] = None
    skills: List[str] = []

class ApplicantProfileUpdate(BaseModel):
    first_name: Optional[str] = None
    last_name: Optional[str] = None
    middle_name: Optional[str] = None
    phone_number: Optional[str] = None
    birth_date: Optional[date] = None
    city: Optional[str] = None
    about:  Optional[str] = None
    skills: Optional[List[str]] = None

class EmployerProfileCreate(BaseModel):
    company_name: str
    description: Optional[str]
    contact: Optional[str]

class EmployerProfileUpdate(BaseModel):
    company_name: Optional[str] = None
    description: Optional[str] = None
    contact: Optional[str] = None

class ApplicantProfileResponse(BaseModel):
    profile_type: Literal[UserRole.APPLICANT] = UserRole.APPLICANT
    user_id: UUID
    first_name: str
    last_name: str
    middle_name: Optional[str]
    phone_number: Optional[str]
    birth_date: Optional[date]
    city: Optional[str]
    about: Optional[str]
    skills: List[str]

    class Config:
        from_attributes = True

class EmployerProfileResponse(BaseModel):
    profile_type: Literal[UserRole.EMPLOYER] = UserRole.EMPLOYER
    user_id: UUID
    company_name: str
    description: Optional[str]
    contact: Optional[str]

    class Config:
        from_attributes = True