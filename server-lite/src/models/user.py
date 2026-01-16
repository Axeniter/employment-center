from core.database import Base
import enum
from sqlalchemy import Column, String, Enum, Boolean, DateTime
from sqlalchemy.sql import func
import uuid
from sqlalchemy.dialects.postgresql import UUID
from sqlalchemy.orm import relationship

class UserRole(str, enum.Enum):
    APPLICANT = "applicant"
    EMPLOYER = "employer"

class User(Base):
    __tablename__ = "users"

    id = Column(UUID(as_uuid=True), primary_key=True, default=uuid.uuid4, index=True)
    email = Column(String, unique=True, index=True, nullable=False)
    hashed_password = Column(String, nullable=False)
    role = Column(Enum(UserRole), nullable=False)
    is_active = Column(Boolean, default=True)
    created_at = Column(DateTime(timezone=True), server_default=func.now())

    applicant_profile = relationship("ApplicantProfile", back_populates="user", uselist=False, cascade="all, delete-orphan")
    employer_profile = relationship("EmployerProfile", back_populates="user", uselist=False, cascade="all, delete-orphan")

    sent_messages = relationship(
        "Message", 
        foreign_keys="Message.sender_id",
        back_populates="sender",
        cascade="all, delete-orphan",
        lazy="dynamic"
    )
    
    received_messages = relationship(
        "Message", 
        foreign_keys="Message.receiver_id",
        back_populates="receiver",
        cascade="all, delete-orphan",
        lazy="dynamic"
    )

    @property
    def profile(self):
        if self.role == UserRole.APPLICANT:
            return self.applicant_profile
        elif self.role == UserRole.EMPLOYER:
            return self.employer_profile
        return None