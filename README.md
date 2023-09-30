# AutoMigration

#### 介绍
基于efcore实现auto migration

#### 使用说明

1. 实现两个必要的接口 IMigrationTableCreator 和 IMigrationDbOperation
2. 使用AddAutoMigration<TDbContext> 添加到容器中
