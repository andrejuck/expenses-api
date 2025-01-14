// Configurações do banco e do usuário
const dbName = "expenses";
const appUser = "admin";
const appPassword = "admin123";

// Conecta ao banco admin para criar o usuário
db = db.getSiblingDB(dbName);

// Cria o usuário com permissões específicas
db.createUser({
  user: appUser,
  pwd: appPassword,
  roles: [{ role: "readWrite", db: dbName }],
});

print(`Usuário ${appUser} criado com sucesso para o banco ${dbName}`);
