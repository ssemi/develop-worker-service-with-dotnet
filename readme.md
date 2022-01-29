# Develop Worker Service with .NET
[[Blog] .NET 기술을 활용한 Worker 서비스 개발기](https://www.ssemi.net/develop-net-core-service-worker/)

### 기본 구성
- Rabbit MQ Subscriber 역할을 하는 worker type들을 전부 모아서 쉽게 운영이 가능하도록 설계
- Publisher (API server) -> RabbitMQ -> Subscriber (Worker)


### **.NET5**
- 각 Worker 는 Project 기반으로 변경
- `config` 폴더에 appsettings / logging 설정 파일을 두도록 수정 config/appsettings.`{environment}`.json
- Publish(게시) 할 때, 불필요한 dll 참조하지 않도록 함


## pre-setup (미리 설정)
```sh
-- rabbitmq setup
$ docker run -d --name rabbitmq -p 5672:5672 -p 15672:15672 -e RABBITMQ_DEFAULT_USER=admin -e RABBITMQ_DEFAULT_PASS=admin rabbitmq:management

# go to management ->  http://localhost:15672/
# authentication
# create queue -> test
```

**Usage** (사용법)
```bat
[each worker project folder]# dotnet run 
```


## Description
- Environment 으로 initialize
`DOTNET_ENVIRONMENT`


### Project Configure
- **Worker-Modules** [class library] : worker 에 필요한 각종 모듈 등록
- workers\ **{worker name}** [console program] : 각각의 worker 프로젝트 본체
- 모든 worker 는 module을 참조하여 기본 기능을 활용한다
