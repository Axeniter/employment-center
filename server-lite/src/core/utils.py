import os
from uuid import UUID
from fastapi import UploadFile, HTTPException
from typing import Optional
from config import config

async def save_upload_file(file: UploadFile, user_id: UUID) -> Optional[str]:
    try:
        if not file.filename:
            raise HTTPException(
                status_code=400,
                detail="No filename provided"
            )
        
        file.file.seek(0, 2)
        file_size = file.file.tell()
        file.file.seek(0)
        
        if file_size > config.MAX_FILE_SIZE:
            raise HTTPException(status_code=400, detail=f"File too large")
        
        file_extension = os.path.splitext(file.filename)[1].lower()
        
        if file_extension not in config.ALLOWED_EXTENSIONS:
            raise HTTPException(status_code=400, detail=f"File type not allowed")

        filename = f"{user_id}{file_extension}"
        
        user_dir = os.path.join(config.UPLOAD_DIR, str(user_id))
        os.makedirs(user_dir, exist_ok=True)
        
        file_path = os.path.join(user_dir, filename)
        if os.path.exists(file_path):
            os.remove(file_path)
        
        with open(file_path, "wb") as buffer:
            content = await file.read()
            buffer.write(content)
        
        return f"/uploads/{user_id}/{filename}"
    
    except HTTPException:
        raise
    except Exception as e:
        raise HTTPException(status_code=500, detail=f"Error saving file: {str(e)}")

def delete_file(file_url: str):
    try:
        if file_url.startswith("/uploads/"):
            file_path = os.path.join(config.UPLOAD_DIR, file_url.replace("/uploads/", "", 1))
            if os.path.exists(file_path):
                os.remove(file_path)
                return True
        return False
    except Exception:
        return False