## 1. Stucture Liquibase's Folder

```bash
/liquibase
?
??? changelog-master.yaml      # Main file including other changelog files
|
??? ticket-8.yaml              # Migration in the order of feature
|
??? LIQUIBASE_GUIDE.md         # Setting guide
?  
??? liquibase.properties       # Liquibase's setting file

## 2. Download Liquibase:
 
   [https://www.liquibase.org/download](https://www.liquibase.org/download)


## 3. Setting liquibase.properties

#Create file `liquibase.properties` at the root project:

url=jdbc:mysql://localhost:3306/your_database_name
username=root
password=your_password
driver=com.mysql.cj.jdbc.Driver

# Path to changelog master
changeLogFile=changelog-master.yaml

# Logging
logLevel=info

## 4. Run Migration

# liquibase update (update database)

# liquibase rollbackCount 1 (rollback the database 1 version)

# liquibase tag course_v1 (tag 1 version)

# liquibase rollback course_v1 (rollback version of the tag)

