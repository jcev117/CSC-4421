#include <stdio.h>
#include <unistd.h>
#include <stdlib.h>
#include <sys/wait.h>
#include <string.h>

int main()
{
	int PID;
	int i=0;
	char lineGot[256];
	char *args[32]={0};
	char *cmd;
	while (1){
		printf("cmd: ");
		fgets(lineGot, 256, stdin); // Get a string from user (includes \n)
		cmd = strtok(lineGot, "\n"); // Get the string without the \n

		if( strcmp(cmd, "e") == 0 ) // loop terminates when "e" is typed
			exit (0);

		args[0]=strtok(lineGot," ");
		args[1]=strtok(NULL," ");
		args[2]=strtok(NULL," ");

		// creates a new process. Parent gets the child's process ID. Child gets 0.
		if ( (PID=fork()) > 0)
		{
			wait(NULL);
		}
		else if (PID == 0) /* child process */
		{
			execvp (cmd, args);
			/* exec cannot return. If so do the following */
			fprintf (stderr, "Cannot execute %s\n", cmd);
			exit(1); /* exec failed */
		}
		else if ( PID == -1)
		{
			fprintf (stderr, "Cannot create a new process\n");
			exit (2);
		}
	}
}