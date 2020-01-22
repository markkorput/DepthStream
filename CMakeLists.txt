cmake_minimum_required(VERSION 3.0.0)
project(DepthStream VERSION 0.1.0)

# make sure we're using at least c++11
include(CheckCXXCompilerFlag)
CHECK_CXX_COMPILER_FLAG("-std=c++11" COMPILER_SUPPORTS_CXX11)
CHECK_CXX_COMPILER_FLAG("-std=c++0x" COMPILER_SUPPORTS_CXX0X)
if(COMPILER_SUPPORTS_CXX11)
    set(CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} -std=c++11")
elseif(COMPILER_SUPPORTS_CXX0X)
    set(CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} -std=c++0x")
endif()

get_filename_component( DepthStream_include "${CMAKE_CURRENT_LIST_DIR}/include" ABSOLUTE )
get_filename_component( DepthStream_src "${CMAKE_CURRENT_LIST_DIR}/src" ABSOLUTE )
FILE(GLOB DepthStream_sources ${DepthStream_src}/*.cpp)

include(CTest)
enable_testing()

add_library(DepthStream ${DepthStream_sources})
target_include_directories(DepthStream PRIVATE ${DepthStream_include})

set(CPACK_PROJECT_NAME ${PROJECT_NAME})
set(CPACK_PROJECT_VERSION ${PROJECT_VERSION})
include(CPack)
