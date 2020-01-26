// #define CATCH_CONFIG_MAIN // <-- is not working on Windows with Visual Studio 2017
#define CATCH_CONFIG_RUNNER
#include "catch.hpp"

int main(int argc, char* argv[])
{
	int result = Catch::Session().run(argc, argv);
	return result;
}