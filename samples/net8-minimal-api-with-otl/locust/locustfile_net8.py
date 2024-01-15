from locust import HttpUser, task, between

class ApiUser(HttpUser):
    wait_time = between(1, 2)

    @task
    def post_balance(self):
        headers = {"Content-Type": "application/json", "Accept": "application/json"}
        data = {
            "procedureName": "WaitForIt",
            "waitSeconds": "1"
        }
        self.client.post("api/Test", json=data, headers=headers)