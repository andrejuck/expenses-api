docker build -f mongodb -t expenses-mongodb . 
docker rm expenses-mongodb
docker run --name expenses-mongodb -d -p 27017:27017 expenses-mongodb
#Para acessar o terminal do container
docker exec -it mongodb bash
#Para realizar operações mongo dentro do container, precisa estar no terminal do container
mongosh -u admin -p admin123 --authenticationDatabase expenses