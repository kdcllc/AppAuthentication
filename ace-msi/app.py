import os
import requests

def get_access_token():
    identity_endpoint = os.environ["IDENTITY_ENDPOINT"]  # Env var provided by Azure. Local to service doing the requesting.
    identity_header = os.environ["IDENTITY_HEADER"]  # Env var provided by Azure. Local to service doing the requesting.
    api_version = "2019-08-01"  # "2018-02-01" #"2019-03-01" #"2019-08-01"
    resource_requested = "https%3A%2F%2Fvault.azure.net"
    client_id = os.environ["AZURE_CLIENT_ID"]  # Env var provided by Azure. Local to service doing the requesting.

    URL = f"{identity_endpoint}?api-version={api_version}&resource={resource_requested}&client_id={client_id}"
    headers = {"X-IDENTITY-HEADER": identity_header}

    try:
        req = requests.get(URL, headers=headers)
    except Exception as e:
        print(str(e))
        return str(e)
    else:
        try:
            password = req.json()["access_token"]
        except:
            password = str(req.text)

    print(password)
    
if __name__ == "__main__":
    get_access_token()
