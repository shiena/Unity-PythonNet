import numpy as np
import matplotlib.pyplot as plt
import ctypes

def draw():
    rand_array = np.random.rand(10000)

    fig = plt.figure()
    ax = fig.add_subplot(1, 1, 1)
    ax.hist(rand_array, bins=20, ec="b")
    
    canvas = plt.get_current_fig_manager().canvas
    canvas.draw()
    buf = canvas.buffer_rgba()
    width, height = canvas.get_width_height()
    pixels = np.frombuffer(buf, dtype=np.uint8).reshape(height, width, 4)
    return np.flipud(pixels).copy()

def getarrayaddr(pixels):
    h, w, s = pixels.shape
    c_addr = pixels.ctypes.data_as(ctypes.POINTER(ctypes.c_uint8 * w * h * s)).contents
    return (ctypes.addressof(c_addr), w, h, s)

if __name__ == '__main__':
    array = draw()
    ptr, w, h, s = getarrayaddr(array)
    print(ptr, w, h, s)