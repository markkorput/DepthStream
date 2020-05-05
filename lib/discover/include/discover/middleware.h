#pragma once

#include <iostream>
#include <functional>

namespace discover { namespace middleware {

  /**
   * This helper class allows "piping" steps using the following notation;
   * Step<int>(3) | double | print | store;
   * where 'double' 'print' and 'store' should be <int> middleware actions
   */
  template <typename D>
  class Step {
    public:

      typedef std::function<D*(D&)> ConvertFunc;
      typedef std::function<bool(D&)> CheckFunc;

    public:

      Step(D* data) : data(data){}
      Step(std::shared_ptr<D> dataRef) : dataRef(dataRef), data(dataRef.get()){}
      ~Step() { this->data = NULL; this->dataRef = nullptr; }

    public:

      /**
       * Continue with result of func. When func returns NULL,
       * it will effectively abort the chained operations.
       */
      inline Step<D>& operator|(ConvertFunc func) { return step(func); }

      inline Step<D>& step(ConvertFunc func) {
        if (data == NULL) return *this;
        this->data = func(*this->data);
        return *this;
      };

      /**
       * If the func returns true, continue with same data,
       * if func returns false, abort (set this->data to NULL
       * will discontinue chained operationss).
       */
      inline Step<D>& operator|(CheckFunc func) { return step(func); }
      inline Step<D>& step(CheckFunc func) { return this->step(toConvert(func)); }

    public:

      static inline ConvertFunc toConvert(CheckFunc func) {
        return [func](D& d) {
          return func(d) ? &d : NULL;
        };
      };
 
    private:

      D* data=NULL;
      std::shared_ptr<D> dataRef=nullptr;
  };

  namespace packet {

    typedef struct {
      const void* data;
      size_t size;
    } Packet;

    using ConvertFunc = Step<Packet>::ConvertFunc;
    using CheckFunc = Step<Packet>::CheckFunc;
    using RawCheckFunc = std::function<bool(const void*,size_t)>;

    inline ConvertFunc to_step(RawCheckFunc func) {
      return Step<Packet>::toConvert([func](Packet& p) {
        return func(p.data, p.size);
      });
    }
    Step<Packet>::ConvertFunc throttle_max_fps(float fps);
  }

  inline Step<packet::Packet> start(const void* data, size_t size) {
    // std::cout << "start " << size << " bytes" << std::endl;
    auto ref = std::make_shared<packet::Packet>();
    *ref = { data, size };
    return Step<packet::Packet>(ref);
  }
}}