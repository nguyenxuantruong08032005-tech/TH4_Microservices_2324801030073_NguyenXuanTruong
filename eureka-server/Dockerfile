# BUILD STAGE
FROM eclipse-temurin:8-jdk-jammy AS build

WORKDIR /workspace/eureka-server

COPY . .

RUN sed -i 's/\r$//' gradlew && \
    chmod +x gradlew && \
    ./gradlew build --no-daemon && \
    ls -lh build/libs

# RUNTIME STAGE
FROM eclipse-temurin:8-jre-jammy

WORKDIR /app

COPY --from=build /workspace/eureka-server/build/libs/eureka-server-0.0.1-SNAPSHOT.jar ./eureka-server.jar

EXPOSE 8761

ENTRYPOINT ["java", "-jar", "/app/eureka-server.jar", "--server.port=8761"]