cmake_minimum_required(VERSION 3.10)
project(DepthStreamTests VERSION 0.1.1 LANGUAGES CXX)

FILE(GLOB DepthStreamTests_sources ${CMAKE_CURRENT_LIST_DIR}/src/*.cpp)
add_executable(DepthStreamTests ${DepthStreamTests_sources})

target_compile_features(DepthStreamTests PRIVATE cxx_auto_type)

# find_package(DepthStream 0.1.1 REQUIRED)
include(${CMAKE_CURRENT_LIST_DIR}/../CMakeLists.txt)
target_link_libraries(DepthStreamTests DepthStream::DepthStream)
