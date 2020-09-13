@echo off

SetLocal

echo This assumes you're running a local docker registry on port 5000. 
echo If you're not, run the following command to start one: 
echo     docker run -d -p 5000:5000 --restart=always --name registry registry:2
set TAG=latest
if NOT [%1]==[] (set TAG=%1) 
@docker build . -f samples\SampleApp.KubernetesApi\Dockerfile -t localhost:5000/sampleapp:%TAG%
@docker push localhost:5000/sampleapp:%TAG%
@kubectl apply -f .\samples\SampleApp.KubernetesApi\sampleapp.yml
@kubectl get pods

EndLocal