# Yerel ışık ve aksiyon kamerası — araştırma, 2026-10-10

Kullanıcı D52'deki yapay koyu bant yerine güneşi kapatıp küçük ışıklarla atmosfer
kurmayı önerdi; araştırma/örnekler ve konuşma istedi. İlk araştırma turunda sahne değiştirilmedi.
Yanıtıyla kabul edilen görünürlük: fener atmosfer versin, fener dışındaki yakın
düşmanlar iki tarafta da seçilsin. Tam karanlık görüş/stealth mekaniği eklenmez.

Mevcut MaquetteCard yalnız ForwardBase içinde yön ışığı + sabit _Ambient hesabı
kullanıyor. Noktasal ışık mesafesi/spot açısı/cookie/gerçek gölge desteği yok.
Dolayısıyla yalnız Directional Light'ı kapatıp SpotLight eklemek düzgün fener
vermez; materyal ışık desteği önce uyarlanmalı. Güneş kapansa da _Ambient
materyal fill'i kaldığı için ortam kendiliğinden tamamen karanlık olmaz.

Önerilen ilk karşılaştırma (bu araştırma yazılırken uygulanmamıştı; sonraki D53
uygulama/kanıt kaydı LOCAL_LIGHTING.md içindedir):
1. Mevcut görünüme dönüş korunarak Survival'da Directional Light kapalı;
   çok düşük soğuk ambient, mat beyaz karton konturları okunur.
2. Tek oyuncu feneri, göğüs/baş yakınından ileri ve hafif aşağı; bakış/nişanla
   döner. Spot ve yumuşak cookie görsel aday. Fener dışında yakın zombiler okunur.
3. Vagon girişlerinde az sayıda sıcak sabit vurgu: tercihen offline vertex
   aydınlatma + emissive küçük lamba yüzeyi. Emission kendi başına çevreyi
   aydınlatmaz. Statik ışık hesapları modele bağlanır, tren hareket kökleri korunur.
4. Yapay abs(z) koyu bant kapalı; vignette/blur çok hafif ya da karşılaştırmada kapalı.
5. Gerçek fenerin metal duvar içinden sızması kontrol edilir. Cookie gölge değildir.
   Gölgesiz ve tek küçük512px gölge hedefli aday kısa A/B ölçülür; bu değer deneme
   önerisidir, cihazda kabul edilmiş bütçe değildir. Çok ışık/çok gölge kullanılmaz.
6. Hasarda kısa küçük yönlü kamera tepkisi; patlamada mesafeye göre sınırlı impulse.
   Her makinelı mermide sürekli shake yok. Hareket yönüne küçük look-ahead.
   Kamera efekti oynanış köklerini/HUD'ı oynatmaz; reduce-motion seçeneği düşünülür.
   Mevcut kamera sahibi RunPresentation'dır; ikinci rakip follow otoritesi kurulmaz.

Kameranın etkisi ışığı kurduktan sonra değerlendirilir. Bu araştırma yeni
teknolojinin APK FPS/ısı maliyetini veya keyfi kanıtlamaz. Sonraki uygulama
önce tek vagonlu ışık denemesi, ardından kısa kalabalık/metal sızıntı kontrolü
ve uygun zamanda A54 ölçümüdür.

Birincil örnekler ve kaynaklar:
- ZeptoLab Bullet Echo resmi sayfası ve galeri:
  https://www.zeptolab.com/games/bullet-echo
  Sınırlı görüş ile ses üzerinden yaklaşım tanıtılıyor. Görsel referanstır;
  onların fenerinin teknik olarak gerçek SpotLight olduğu bilinmiyor.
- Unity6.6 Light.cookie:
  https://docs.unity3d.com/6000.6/Documentation/ScriptReference/Light-cookie.html
  Cookie ışığın yansıttığı desendir; pixel/baked/mixed desteği, vertex/SH sınırı.
- Unity6.6 Forward rendering:
  https://docs.unity3d.com/6000.6/Documentation/Manual/RenderTech-ForwardRendering.html
  Etkileyen ek per-pixel ışıklar ek geçişler doğurur; bir büyük birleşik renderer
  çok ışıkla kesişebilir. Birleştirme ile ışık maliyeti birlikte düşünülür.
- Unity Cinemachine Impulse ve örnekleri:
  https://docs.unity3d.com/Packages/com.unity.cinemachine@3.1/manual/CinemachineImpulse.html
  https://docs.unity3d.com/Packages/com.unity.cinemachine@3.1/manual/samples-tutorials.html
  Olayın yönü, şiddet/zaman eğrisi ve mesafe ile kamera tepkisi.
  Projede com.unity.cinemachine6.6.0 var; kaynak3.1 kavram örneğidir, birebir
  API sürümü varsayılmayacak, uygulamada yerel6.6 paket dokümanı okunacak.
- Squirrel Eiserloh, GDC2016, Juicing Your Cameras With Math:
  https://www.gdcvault.com/play/1023557/Math-for-Game-Programmers-Juicing
  Yumuşak takip, kadraj ve kontrollü shake konusunda izlenebilir birincil sunum.
