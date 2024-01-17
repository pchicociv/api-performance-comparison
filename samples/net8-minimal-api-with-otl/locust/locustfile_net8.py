from locust import HttpUser, task, between

class ApiUser(HttpUser):
    wait_time = between(1, 2)

    @task
    def post_balance(self):
        headers = {"Content-Type": "application/json", "Accept": "application/json", "Authorization": "Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1bmlxdWVfbmFtZSI6IlBlZHJvQ2hpY28iLCJzdWIiOiJQZWRyb0NoaWNvIiwianRpIjoiMzEzN2FjIiwiYXVkIjpbImh0dHA6Ly9sb2NhbGhvc3Q6MzQwMTIiLCJodHRwczovL2xvY2FsaG9zdDowIiwiaHR0cDovL2xvY2FsaG9zdDo1Mjg1Il0sIm5iZiI6MTcwNTQyNTU1NCwiZXhwIjoxNzEzMjg3OTU0LCJpYXQiOjE3MDU0MjU1NTUsImlzcyI6ImRvdG5ldC11c2VyLWp3dHMifQ.bjt29pE-Ef_CVj-OZoiBEEGrZCoMztMkrr6TG_mtmaQ"}
        data = {
            "procedureName": "WaitForIt",
            "waitSeconds": "1"
        }
        self.client.post("api/Test", json=data, headers=headers)