#include <stdio.h>
#include <sys/wait.h>
#include <time.h>
#include <unistd.h>

int main()
{

int time=0;


printf("Enter the number of seconds you want this program to run: ");
scanf(" %d",&time);

int pid=fork();

if(time<1)
{
    printf("Error: Incorrect time entry \n");
}
if(pid==0)
{
    for(int i=0;i<time;i++)
    {
        printf("hello %d\n",i);
        sleep(1);
    }
}
else
{
    wait(NULL);
    printf("Process ended after %d seconds \n", time);

}
 

return 0;
}