# Survival maket ışığı — D53, 2026-10-10

Kullanıcı güneşi kapatıp küçük ışıkları denemeyi istedi. Fener dışındaki yakın
düşmanlar da iki tarafta okunur; görüş kısıtlaması veya yeni savaş kuralı yok.
Referanslar atmosfer içindir. Tren ölçeği, oynanış fiziği, kapı kimliği ve NavMesh
değiştirilmez. Deneme yalnız SurvivalIntegration sahnesindedir.

## Kurulan görünüm

- Directional Light kapalı. Önceki world-Z karartma bandı kaldırıldı. Çevre kartı
  düşük soğuk fill, oyuncu/düşman kartları ayrı okunurluk fill'i kullanır.
- Oyuncunun köküne bağlı tek gerçek Spot Light: yerel (0,1.25,.32)m,16° aşağı,
  8m menzil,85° koni,1.8 yoğunluk. PlayerMotor'un bakış yönünü doğrudan izler;
  ikinci takip/nişan sistemi ve karelik ışık araması yok.
- Özgün128×128 alfa cookie kenarı yumuşatır. Cookie gölge değildir. Fener tek
  512px hard shadow kullanır; gövde/dış kapı metali gölge atar, çevre/aktörler
  alır. Cam açıklıkları gövde mesh'inin gerçek delikleridir. Oyuncu kendi
  fenerini gölgelemez. Inspector'daki Cast Metal Shadows karşılaştırma içindir;
  kapatmak duvar arkasında ışık sızıntısı oluşturabilir.
- Her vagonda iki sıcak,4.5m menzilli ForceVertex point light: ek pixel-light
  geçişi/gölge haritası yok. Bu vurgu ışıkları gölgesiz stil aydınlatmasıdır;
  ışığın duvar engeli kontrolü yalnız oyuncu fenerindedir. Ayrı küçük lamba
  lensleri ve offline bake bu ilk denemede eklenmedi.
- MaquetteCard mat Lambert hesabını korur. ForwardBase içinde fill+vertex
  ışıklar; fener ForwardAdd içinde Unity cookie/mesafe/gölge azaltması.
  ShadowCaster geçişi. PBR, reflection probe, volumetric fog, HDR veya yeni
  kamera yok. Mevcut384px sınırlı tilt-shift ve65ms kamera takibi korunur.

RenderSettings ortam rengi Standard kullanan mevcut parçaları etkiler;
MaquetteCard kendi _Ambient değerini kullanır. Shader değişikliği shader'ın
kullanıcılarını etkiler; Battle sahnesi mevcut diğer materyallerini kullanır.
Global QualitySettings değiştirilmez. Çok düşük/Low kalitede Unity gölgeleri
kapalıdır; yayın için bu seçenek ayrıca metal ışık sızıntısı açısından kontrol
edilmeli. Varsayılan Android Medium tek pixel light ve hard shadows destekler.

## Yeniden kurma ve kanıt

Play durmuşken Wait Your Turn → Survival → Install Local Maquette Lights.
Tekrar çalıştırmak aynı ışıkları günceller, çoğaltmaz. D51/D52 kurulumlarını
tekrar çalıştırmak malzemeyi/güneşi sıfırlayabilir; ardından bu kurulumu yapın.
Geri dönüş Git'te önceki c0ff5bc sürümüdür; NavMesh yeniden pişirilmez.

Editor tek-sefer komutları Temp/maquette-lighting.request: install, preview,
check (Play'de mevcut kritik tren kontrolü), budget (Play'de kısa24-zombi
yürüyüş/gölge OFF-ON karşılaştırması). Bunlar test mağazası kullanır; normal
koşu kayıt dosyasını değiştirmez. Eski karşılaştırma görselleri korunur.

Üretilen kanıtlar docs/generated/local-lighting-*. Shader derleme/PC kısa
performans kabulü Android GPU,ısı,pil veya reklam dönüşüm kanıtı değildir.

Kısa sonuç:15kapı/15cam/iki fiziksel geçit PASS, önceki sahneye göre292
collider/agent/obstacle aynı; bütün eski transform konum/dönüş/ölçek/parent
değerleri korunur. Play Console0hata/0uyarı. Önceki23Unity API obsolete
derleme uyarısı mevcut sandbox/check kodundadır; fener shader hatası yok.
24zombi,1018×588,Ultra Editor yürüyüş örneği: gölge açık ortalama6.25ms,
P958.36ms; kapalı önceki örnek7.62ms/P9510.46ms. Sıralı kısa örnekler nedeniyle
bu fark gölge açınca hızlandığımızı göstermez; GPU maliyet ayrıştırması değildir.
GC16KB/kare Editor dahil;0 döndüren draw counter kullanışlı ölçüm vermedi,
0draw veya batching kazancı olarak yorumlanmaz. Test yaklaşık10s sürer.

## Asset değerlendirmesi — paket eklenmedi

2026-10-10 tarihinde resmi Asset Store uyumluluk tabloları kontrol edildi:

| Paket | Mevcut Built-in proje | İlk kullanım kararı |
| --- | --- | --- |
| [Miniature Camera](https://assetstore.unity.com/packages/vfx/shaders/fullscreen-camera-effects/miniature-camera-tilt-shift-camera-effect-375230) | Uyumsuz; URP destekli | Önce mevcut hafif efekti deneyelim; pipeline göçü yapmayalım. |
| [Grid Master](https://assetstore.unity.com/packages/vfx/shaders/grid-master-286714) | Uyumsuz; URP destekli | Ayrı deneme projesinde içerik/dokular incelensin; uygun görüntüler mevcut materyale uyarlanabilir. Paketin içinde hangi dokuların bulunduğu henüz görülmedi. |
| [TopDown Engine](https://assetstore.unity.com/packages/templates/systems/topdown-engine-89636) | Güncel5.0 Built-in destekli | Kullanıcının eski kopyası farklı olabilir. Kamera/ışık demo fikri incelenebilir; bütün oynanış motoru mevcut projeye aktarılmaz. |

Paket dosya boyutu çalışma zamanı shader/pixel/gölge maliyetini kanıtlamaz.
Fog referansının birinde uzaklık kameradan, diğerinde figürden başlıyor denmiş;
görsel fark sahnedeki figür çevresinin görünür kalma biçimidir. Ekran görüntüsü
tek başına kullanılan shader'ın maliyetini veya bizim pipeline desteğini vermez.
Bu tur fog asseti veya URP sistemi eklenmedi.

Sonraki sıra: kullanıcı ışık denemesi → gerekirse kontrollü Grid Master içerik
incelemesi → ışık oturunca küçük olay bazlı kamera tepkisi ve maket aktörler →
A54 kalabalık GPU/FPS/ısı ölçümü. Yeni kamera sahibi kurulmamalı.
