# Use an official Python runtime as a parent image
# az acr build --registry acrshcqa --image msitest:v1 --file Dockerfile .
FROM python:3.9-slim

# Set the working directory in the container
WORKDIR /app

# Copy the current directory contents into the container at /app
COPY . /app

# Install any needed packages specified in requirements.txt
RUN pip install --no-cache-dir -r requirements.txt

# Make port 80 available to the world outside this container
EXPOSE 80


# Update the CMD to run both python app.py and tail -f /dev/null
CMD ["/bin/sh", "-c", "python app.py && tail -f /dev/null"]
