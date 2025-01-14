docker build -f mongodb  -t expenses-mongodb . 
docker rm expenses-mongodb
docker run --name expenses-mongodb -d -p 27017:27017 expenses-mongodb
 
mongosh -u admin -p admin123 --authenticationDatabase expenses