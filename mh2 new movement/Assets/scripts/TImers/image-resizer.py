from PIL import Image
import os

input_folder = 'input_images'
output_folder = 'output_images'
crop_width = 1020
crop_height = 590

os.makedirs(output_folder, exist_ok=True)

for filename in os.listdir(input_folder):
    if filename.lower().endswith(('.png', '.jpg', '.jpeg', '.gif')):
        path = os.path.join(input_folder, filename)
        image = Image.open(path)
        
        w, h = image.size
        left = (w - crop_width) // 2
        top = (h - crop_height) // 2
        right = left + crop_width
        bottom = top + crop_height
        
        cropped = image.crop((left, top, right, bottom))
        cropped.save(os.path.join(output_folder, filename))
        
        print(f"cropped {filename}")