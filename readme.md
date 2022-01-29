# Develop Worker Service with .NET
[[Blog] .NET 기술을 활용한 Worker 서비스 개발기](https://www.ssemi.net/develop-net-core-service-worker/)

### **.NET Core 3.1**
- Rabbit MQ Subscriber 역할을 하는 worker type들을 전부 모아서 쉽게 운영이 가능하도록 설계
- Publisher (API server) -> RabbitMQ -> Subscriber (Worker)
- appsetting.`{workerType}`.`{environment}`.json 으로 환경 분리
- worker type 을 통해 필요한 worker로의 변경
- 각 기능은 모두 project 별로 나뉨


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
worker.exe --env `{environment}` --workertype `{workerType}` --queue `{queueName}`
```


## Description
- Environment 으로 initialize
`ASPNETCORE_ENVIRONMENT`
`WORKER_TYPE`

두 개의 환경 변수로 초기화를 진행
CommandOptions 값이 있을 경우 옵션 값을 우선으로 셋업

```
--env = ASPNETCORE_ENVIRONMENT
--workertype = WORKER_TYPE
--queue = 개발 편하게 하려고 추가한 옵션 큐
Environment, workerType 필수 값
OptionQueue 값은 개발 테스트 하기 좋게 추가
```

### Project Configure
- **Worker** [console program] : Program 시작점 및 Runner.cs 파일로 실제 Run
- **Worker-Modules** [class library] : worker 에 필요한 각종 모듈 등록
- workers\ **{worker name}** [class library] : 각각의 worker 프로젝트 

