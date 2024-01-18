from locust import HttpUser, task, between

class ApiUser(HttpUser):
    wait_time = between(1, 2)

    @task
    def post_balance(self):
        headers = {"Content-Type": "application/json", "Accept": "application/json"}
        data = {
            "username": "MyUsername",
            "password": "MyStr0ngP@ssw0rd"
        }
        self.client.post("api/CreateUser", json=data, headers=headers)