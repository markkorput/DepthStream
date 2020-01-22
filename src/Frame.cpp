#include "DepthStream/Frame.h"

using namespace depth;

void WritableFrame::concat(const Frame &f1, const Frame &f2) {
  size_t newsize = f1.size() + f2.size();

  if (this->ownedData == NULL || this->_size < newsize) {
    // free existing block
    if (this->ownedData) free(this->ownedData);
    // allocate new block
    ownedData = malloc(f1.size() + f2.size());
    _size = newsize;
  }

  // first fame at start of our data block
  this->write(f1.data(), f1.size(), 0);
  // second frame right after it
  this->write(f2.data(), f2.size(), f1.size());
}