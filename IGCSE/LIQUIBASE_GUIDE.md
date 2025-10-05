## ?? 1. C?u Trúc Th? M?c Liquibase

```bash
/liquibase
?
??? changelog-master.yaml      # File g?c (master) ?? include t?t c? các file khác
|
??? ticket-8.yaml              # Migration theo th? t? tính n?ng (VD: t?o b?ng course feat/#8)
|
??? LIQUIBASE_GUIDE.md         # File h??ng d?n này
?  
??? liquibase.properties       # C?u hình k?t n?i liquibase

## ?? 2. Cài ??t Liquibase

### ? Dùng CLI tr?c ti?p

1. T?i Liquibase t?i:  
   ?? [https://www.liquibase.org/download](https://www.liquibase.org/download)

2. Gi?i nén và thêm vào **PATH** (?? có th? gõ `liquibase` t? terminal).

3. Ki?m tra cài ??t:

```bash
liquibase --version


## ?? 3. C?u Trúc liquibase.properties

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

# liquibase rollbackCount 1 (quay l?i 1 changeset tr??c ?ó)

# liquibase tag course_v1 (tag 1 version)

# liquibase rollback course_v1 (rollback version ?ó)

