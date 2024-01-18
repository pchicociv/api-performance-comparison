from locust import HttpUser, task, between

class ApiUser(HttpUser):
    wait_time = between(1, 2)

    @task
    def post_balance(self):
        headers = {"Content-Type": "application/json", "Accept": "application/json", "Authorization": "Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1bmlxdWVfbmFtZSI6IlBlZHJvQ2hpY28iLCJzdWIiOiJQZWRyb0NoaWNvIiwiaWF0IjoxNzA1NTc2NjE4LCJpc3MiOiJodHRwOi8vbWluaW1hbGFwaS5uZXQiLCJhdWQiOiJodHRwOi8vYXVkaWVuY2UuY29tIn0.GJOAUl3oRkh45xa7zYcgeMws8nw1lLu8ji1XG-FFagU"}
        data = {
            "procedureName": "WaitForIt",
            "waitSeconds": "1"
        }
        self.client.post("api/CallProcedureWithJwt", json=data, headers=headers)