## ?? 1. C?u Tr�c Th? M?c Liquibase

```bash
/liquibase
?
??? changelog-master.yaml      # File g?c (master) ?? include t?t c? c�c file kh�c
|
??? ticket-8.yaml              # Migration theo th? t? t�nh n?ng (VD: t?o b?ng course feat/#8)
|
??? LIQUIBASE_GUIDE.md         # File h??ng d?n n�y
?  
??? liquibase.properties       # C?u h�nh k?t n?i liquibase

## ?? 2. C�i ??t Liquibase

### ? D�ng CLI tr?c ti?p

1. T?i Liquibase t?i:  
   ?? [https://www.liquibase.org/download](https://www.liquibase.org/download)

2. Gi?i n�n v� th�m v�o **PATH** (?? c� th? g� `liquibase` t? terminal).

3. Ki?m tra c�i ??t:

```bash
liquibase --version


## ?? 3. C?u Tr�c liquibase.properties

#T?o file `liquibase.properties` ? th? m?c g?c c?a project:

url=jdbc:mysql://localhost:3306/your_database_name
username=root
password=your_password
driver=com.mysql.cj.jdbc.Driver

# Path to changelog master
changeLogFile=changelog-master.yaml

# Logging
logLevel=info

## ?? 4. Ch?y Migration

# liquibase update (update database)

# liquibase rollbackCount 1 (quay l?i 1 changeset tr??c ?�)

# liquibase tag course_v1 (tag 1 version)

# liquibase rollback course_v1 (rollback version ?�)

