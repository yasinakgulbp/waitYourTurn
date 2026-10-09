"""Generate packed PBR maps and convert the approved albedo source to runtime JPEG.
Source image is retained outside Assets; a procedural fallback is available.
"""
from pathlib import Path
import numpy as np
from PIL import Image, ImageDraw, ImageFont, ImageFilter

ROOT = Path(__file__).resolve().parents[2]
OUT = ROOT / 'Assets/Game/Art/SurvivalTrain/Textures'
OUT.mkdir(parents=True, exist_ok=True)
SIZE, TILE = 2048, 512
rng = np.random.default_rng(47)
albedo = Image.new('RGB', (SIZE, SIZE))
packed = Image.new('RGBA', (SIZE, SIZE))
height = Image.new('L', (SIZE, SIZE), 128)
colors = [(139,143,138),(119,112,96),(32,37,40),(155,29,24),
          (66,72,72),(28,34,37),(159,164,161),(171,132,51),
          (144,29,24),(106,113,113),(220,179,87),(30,62,68),
          (43,49,51),(46,47,43),(39,90,82),(122,128,121)]
for index, color in enumerate(colors):
    yy, xx = np.mgrid[:TILE,:TILE]
    grain = rng.normal(0, 2.4, (TILE,TILE))
    brushed = np.sin(yy*.55)*1.1 + rng.normal(0,1,(TILE,1))
    edge = np.minimum.reduce([xx,yy,TILE-1-xx,TILE-1-yy])
    dark = np.clip((21-edge)/21,0,1)*19
    noise = grain + brushed - dark
    pix = np.clip(np.asarray(color)[None,None,:]+noise[:,:,None],0,255).astype('uint8')
    im = Image.fromarray(pix); d = ImageDraw.Draw(im)
    hi = Image.new('L',(TILE,TILE),128); h = ImageDraw.Draw(hi)
    # Subtle aged paint: restrained stains, scratches, panel creases, bolt heads.
    for _ in range(95):
        x,y = rng.integers(10,TILE-10,2); length = int(rng.integers(2,24))
        shade = tuple(max(0,c-int(rng.integers(7,18))) for c in color)
        d.line((int(x),int(y),int(x+length),int(y-1)),fill=shade,width=1)
    if index in (0,1,3,8,9,15):
        # Multiscale dirt, tarnish and rubbed paint live in texture, not extra polygons.
        dirt=rng.normal(0,1,(32,32)); dirt=Image.fromarray(np.clip(dirt*24+128,0,255).astype('uint8')).resize((TILE,TILE),Image.Resampling.BICUBIC).filter(ImageFilter.GaussianBlur(9))
        cloud=np.asarray(dirt,dtype=np.float32)-128
        arr=np.asarray(im,dtype=np.float32)
        arr+=cloud[:,:,None]*.24
        im=Image.fromarray(np.clip(arr,0,255).astype('uint8'));d=ImageDraw.Draw(im)
        for _ in range(34):
            x=int(rng.choice([rng.integers(4,25),rng.integers(486,508)]));y=int(rng.integers(8,498))
            d.ellipse((x,y,x+int(rng.integers(2,7)),y+int(rng.integers(4,21))),fill=(94,76,53))
        d.rectangle((7,7,504,504),outline=tuple(max(0,c-35) for c in color),width=3)
        d.line((11,12,501,12),fill=tuple(min(255,c+26) for c in color),width=2)
        for x in (18,494):
            for y in (20,252,492):
                d.ellipse((x-4,y-4,x+4,y+4),fill=(35,39,39))
                d.ellipse((x-3,y-3,x+2,y+2),fill=(155,157,148))
                h.ellipse((x-4,y-4,x+4,y+4),fill=200)
    if index == 4:
        # Fine treadplate plus worn panel borders; geometry stays flat.
        for y in range(12,512,23):
            for x in range(12,512,23):
                a = x+(11 if (y//23)%2 else 0)
                d.line((a,y,a+8,y+5),fill=(105,111,108),width=2)
                d.line((a,y+3,a+8,y+8),fill=(39,46,47),width=2)
                h.line((a,y,a+8,y+5),fill=171,width=3)
        d.rectangle((2,2,509,509),outline=(26,32,34),width=4)
    if index in (5,12):
        for y in range(9,512,15):
            d.rectangle((9,y,502,y+6),fill=(13,18,20)); d.line((9,y+7,502,y+7),fill=(78,86,86),width=2)
            h.rectangle((9,y,502,y+6),fill=85)
    if index == 7:
        for x in range(-512,1024,100):
            d.polygon([(x,0),(x+42,0),(x+554,512),(x+512,512)],fill=(34,37,36))
    if index == 8:
        # Door red lower panels: recessed insets, no fake metal in the shot window.
        for x in (25,270):
            d.rounded_rectangle((x,45,x+216,432),radius=13,outline=(66,18,17),width=6)
            d.line((x+7,52,x+207,52),fill=(191,58,43),width=2)
            for y in range(278,398,17):
                d.rectangle((x+16,y,x+195,y+7),fill=(42,27,27))
                h.rectangle((x+16,y,x+195,y+7),fill=90)
    if index == 14:
        for y in (45,72,112,330): d.line((26,y,445,y),fill=(84,187,145),width=3)
    if index == 15:
        font=ImageFont.truetype('C:/Windows/Fonts/arialbd.ttf',54)
        d.text((42,145),'AUTOMATED',font=font,fill=(38,43,43))
        d.text((115,213),'TRANSIT',font=font,fill=(38,43,43))
        d.line((40,298,470,298),fill=(134,42,34),width=15)
    smooth = {2:50,4:80,5:60,6:185,7:65,10:160,12:65}.get(index,110)
    metal = {1:95,2:0,3:55,7:30,8:50,10:30,11:0,13:0,14:0}.get(index,175)
    mask = Image.new('RGBA',(TILE,TILE),(metal,0,0,smooth))
    px,py = (index%4)*TILE,(index//4)*TILE
    albedo.paste(im,(px,py)); packed.paste(mask,(px,py)); height.paste(hi,(px,py))

# Tangent-space normal derived from the authored relief; preserve linear channels.
hh=np.asarray(height.filter(ImageFilter.GaussianBlur(.65)),dtype=np.float32)/255
gy,gx=np.gradient(hh); normal=np.dstack((-gx*3,-gy*3,np.ones_like(hh)))
normal/=np.linalg.norm(normal,axis=2,keepdims=True)
normal=np.clip((normal*.5+.5)*255,0,255).astype('uint8')
source = ROOT/'ArtSource/SurvivalTrain/TrainAtlas_Albedo_Source.png'
if source.exists():
    albedo = Image.open(source).convert('RGB')
albedo.save(OUT/'TrainAtlas_Albedo.jpg',quality=93,subsampling=0,optimize=True)
packed.save(OUT/'TrainAtlas_MetalSmooth.png',optimize=True)
Image.fromarray(normal,'RGB').save(OUT/'TrainAtlas_Normal.png',optimize=True)
print('Shared atlas:',[(p.name,p.stat().st_size) for p in OUT.iterdir()])
