# Manual end-to-end smoke test

Two test paths are provided: a `.http` file for VS Code REST Client / Rider, and a
shell script with `curl` commands for terminals.

## Prerequisites

1. SQL Server reachable. Easiest: `docker run --name edtech-sql -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=Your_password123" -p 1433:1433 -d mcr.microsoft.com/mssql/server:2022-latest`
2. Apply migrations: `dotnet ef database update --project src/EdTech.SheetIntegrator.Infrastructure`
3. Run the API: `dotnet run --project src/EdTech.SheetIntegrator.Api`
   (default base URL `http://localhost:5000`; OpenAPI UI at `http://localhost:5000/docs`)

## Path A — `.http` file (recommended)

Open [`src/EdTech.SheetIntegrator.Api/EdTech.SheetIntegrator.Api.http`](../src/EdTech.SheetIntegrator.Api/EdTech.SheetIntegrator.Api.http)
in VS Code (with the REST Client extension) or Rider, and run requests 1–11 in order.
Variables (`{{token}}`, `{{assessmentId}}`, `{{submissionId}}`) chain through automatically.

## Path B — `curl`

```bash
HOST=http://localhost:5000

# 1. Liveness
curl -i $HOST/health/live

# 2. Mint a dev token
TOKEN=$(curl -s -X POST $HOST/dev/token \
  -H "Content-Type: application/json" \
  -d '{"subject":"gabe@local"}' | jq -r .token)
echo "TOKEN=$TOKEN"

# 3. Create an assessment
ASSESSMENT_ID=$(curl -s -X POST $HOST/api/v1/assessments \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "title":"Geography & math quiz",
    "questions":[
      {"questionId":"Q1","prompt":"Capital of France?","correctAnswer":"Paris","points":1,"matchMode":"exact","numericTolerance":null},
      {"questionId":"Q2","prompt":"Pi to two decimals?","correctAnswer":"3.14","points":5,"matchMode":"numeric","numericTolerance":0.01}
    ]
  }' | jq -r .id)
echo "ASSESSMENT_ID=$ASSESSMENT_ID"

# 4. Read assessment (open)
curl -s $HOST/api/v1/assessments/$ASSESSMENT_ID | jq

# 5. Upload a CSV submission
printf "QuestionId,Response\nQ1,Paris\nQ2,3.14\n" > /tmp/alice.csv
SUBMISSION_ID=$(curl -s -X POST $HOST/api/v1/assessments/$ASSESSMENT_ID/submissions \
  -H "Authorization: Bearer $TOKEN" \
  -F "studentIdentifier=alice@example.com" \
  -F "file=@/tmp/alice.csv;type=text/csv" | jq -r .id)
echo "SUBMISSION_ID=$SUBMISSION_ID"

# 6. Read graded submission
curl -s $HOST/api/v1/submissions/$SUBMISSION_ID | jq

# 7. List submissions
curl -s "$HOST/api/v1/assessments/$ASSESSMENT_ID/submissions?page=1&pageSize=20" | jq

# 8. Negative: 415 unsupported file type
echo "garbage" > /tmp/alice.txt
curl -i -X POST $HOST/api/v1/assessments/$ASSESSMENT_ID/submissions \
  -H "Authorization: Bearer $TOKEN" \
  -F "studentIdentifier=alice@example.com" \
  -F "file=@/tmp/alice.txt;type=text/plain"
```

## What you should see

| Step | Expected status | Body shape |
|---|---|---|
| Liveness | 200 | empty |
| Mint token | 200 | `{ "token": "...", "subject": "gabe@local" }` |
| Create assessment | 201 + `Location` header | `{ "id": "...", "title": "...", "maxScore": 6, "questions": [...] }` |
| Get assessment | 200 | same shape as create |
| Submit (csv) | 201 + `Location` header | `{ "id": "...", "isGraded": true, "earned": 6, "total": 6, "percentage": 100, "outcomes": [...] }` |
| Get submission | 200 | same as submit |
| List submissions | 200 | `{ "items": [...], "page": 1, "pageSize": 20, "totalCount": 1, "totalPages": 1, "hasNextPage": false }` |
| Unsupported file | 415 | RFC 7807 ProblemDetails with `code: "submission.unsupported_file_type"` |
