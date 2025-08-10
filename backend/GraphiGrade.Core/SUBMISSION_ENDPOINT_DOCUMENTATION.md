# Submission API Endpoint

## Overview
The submission endpoint allows users to submit solutions for exercise judging.

## Endpoint
`POST /api/Exercise/{exerciseId}/submit`

## Request Model
```json
{
  "sourceCodeBase64": "base64-encoded-source-code",
  "fileExtension": ".cs"
}
```

### Request Properties
- `sourceCodeBase64` (required): Base64 encoded source code content
- `fileExtension` (optional): File extension for the source code (e.g., ".cs", ".py", ".java"). Defaults to ".txt" if not specified.

## Response Model
```json
{
  "submissionId": "unique-submission-id-from-judge-service",
  "status": "Queued|Running|Finished|NotQueued",
  "submittedAt": "2023-12-07T10:30:00Z"
}
```

## Authorization
- User must be authenticated
- User must have access to the exercise (either admin or belongs to a group that has access to the exercise)

## Example Usage

### Request
```bash
curl -X POST "https://localhost:7000/api/Exercise/1/submit" \
  -H "Authorization: Bearer {your-jwt-token}" \
  -H "Content-Type: application/json" \
  -d '{
    "sourceCodeBase64": "Y29uc29sZS5sb2coIkhlbGxvIFdvcmxkISIp",
    "fileExtension": ".cs"
  }'
```

### Response
```json
{
  "submissionId": "550e8400-e29b-41d4-a716-446655440000",
  "status": "Queued",
  "submittedAt": "2023-12-07T10:30:00Z"
}
```

## File Type Support

The system now supports different file types with proper content type handling:

### Supported File Extensions
- `.cs` - C# source code
- `.py` - Python source code
- `.java` - Java source code
- `.js` - JavaScript source code
- `.ts` - TypeScript source code
- `.cpp`, `.cc`, `.cxx` - C++ source code
- `.c` - C source code
- `.h` - C/C++ header files
- `.html` - HTML files
- `.css` - CSS files
- `.json` - JSON files
- `.xml` - XML files
- `.sql` - SQL files
- `.txt` - Plain text files (default)

## Error Responses

### 400 Bad Request
- Source code size exceeds the maximum allowed limit
- Invalid exercise ID (? 0)

### 403 Forbidden
- User is not authenticated
- User doesn't have access to the exercise

### 404 Not Found
- Exercise with the specified ID doesn't exist

### 500 Internal Server Error
- Database connection issues
- Judge service unavailable
- Blob storage errors

## Implementation Details

The endpoint:
1. Validates the user's authorization to access the exercise
2. Stores the source code in Azure Blob Storage using the appropriate content type
3. Creates file metadata with the correct file type (FileType.SourceCode)
4. Retrieves the expected pattern from the exercise
5. Submits both to the external judge service
6. Creates a submission record in the database
7. Returns the submission details

### File Type Enum
The system uses a `FileType` enum to categorize stored files:
- `Image = 0` - Exercise expected result images
- `SourceCode = 1` - User submission source code
- `ResultImage = 2` - Judging result images

The judge service processes submissions asynchronously, so the initial response will typically show a "Queued" status. Users can poll for status updates using a separate endpoint (to be implemented).