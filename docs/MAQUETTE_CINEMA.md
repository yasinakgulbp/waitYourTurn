# Maket sineması — D52 (2026-10-10)

SurvivalIntegration'a uygulandı. Referansın minyatür hissi; aydınlık mat karton tren,
koyu mavi uzak çevre, ölçülü sıcak vurgu ve seçici bulanıklıkla deneniyor.
Bu görüntü gerçek Unity kamerasındandır: generated/cinema-unity-preview.png.
Kullanıcının estetik değerlendirmesi ve Android kabulü henüz tamamlanmadı.

## Yöntem ve maliyet sınırı

- Card shader: rayın iki yanına uzaklıkla 2.8–6.8m arasında koyulaşan çevre.
  Kamera yürürken karanlık bant trenin çevresinde kalır. Trenin yakını okunur.
- Mevcut tek yön ışığı, hafif sıcak renk; vertex AO ve analitik taban/duvar
  temas tonu. Yeni realtime gölge, SSAO, volumetric fog veya HDR yok.
- MaquetteCinema: Built-in OnRenderImage, iki düşük çözünürlüklü Gaussian
  geçiş + tek tam ekran renk/vignette birleştirmesi. Blur çözünürlüğü ekranın
  dörtte biri; uzun kenar en fazla384px. İki geçici RenderTexture havuzdan
  alınır/geri verilir. 384x216 RGBA8 için yaklaşık648KiB blur tamponu;
  normal kamera/postprocess renk hedefleri buna ek olarak motor tarafından yönetilir.
- Bu fiziksel DOF değildir: dört dünya noktasından hesaplanan odak bandı,
  tren/oyuncu/yakın saldırganları net tutar; dış kenarlarda seçici tilt-shift.
  UI efekt sonrasında çizilir. Depth texture/prepass veya ikinci sahne kamerası yok.
- Inspector'da Use Blur=false veya Very Low kalite seviyesi blur geçişlerini
  kaldırır; renk/vignette kalır. Component kapatılınca efekt bütünüyle kalkar.
  Blur Amount=.55, Vignette=.22. A54 GPU/kare/ısı testi hâlâ gerekli.
- RunPresentation kamera LateUpdate'i PlayerMotor sonrası çalışır. Survival
  takip gecikmesi65ms; zaman tabanlı yumuşatma, büyük teleportta doğrudan geçiş.
  Battle sahnesine kamera efekti/lag kurulmadı; ortak sunum scriptindeki
  execution order değişikliğinin tam Battle kabulü ayrıca bekler.

[Unity'nin mobil postprocess rehberi](https://docs.unity.com/en-us/engine/6000.3/manual/post-processing-and-full-screen-effects/urp/integration-with-post-processing)
Gaussian DOF, renk ve vignette için mobil öneriler verir; bokeh daha ağırdır.
Bu kaynak URP içindir; proje Built-in kaldığından sınırlandırılmış özel çözüm
uygulandı. Mobil öneri cihazımızda sıfır maliyet veya60FPS garantisi değildir.

## Parça, titreşim ve geometri

D51'de üç vagonun her gövdesi ve iki körüğün her biri zaten tek mesh/renderer.
Kapı yaprakları ve camlar bağımsız. Körük çizgileri ayrı renderer değildir.
802 eski, kapalı MeshRenderer/MeshFilter çifti sahneden kaldırıldı; GameObject,
Collider, NavMeshAgent/Obstacle ve oyun kimlikleri kaldı. Bu802 ekstra draw call
anlamına gelmez: kapalı rendererlar önceden de çizilmiyordu.

HEAD1754a0e ile YAML karşılaştırması292 fizik/nav componentinin değişmediğini,
yalnız yön ışığının transformunun değiştiğini ve silinen componentlerin sadece
802Renderer+802MeshFilter olduğunu doğrular. NavMesh varlığı değişmedi.
Kanıt: generated/cinema-geometry-check.txt. Camera near/far1/80m; kamera
zamanlaması ve takibi düzeltildi. Mesh sayısı azaltmak tek başına z-fighting'i
çözmez; aynı düzlemde üst üste yüzey veya ince çizgi aliasing'i ayrı konulardır.
Bu tur bağımsız fizik parçalarının kayması saptanmadı; bütün olası kamera/aspect
ve telefon çözünürlüklerinde titreşimin bittiği iddia edilmiyor.

## Kısa gerçek kontroller ve bulunan PC kasması

- Unity compile/shader import başarılı. Son Play Console0 hata/0 uyarı.
- 15kapı cam/metal ışını, kırılma/onarım/görsel geri dönüşü;15yan pencere
  çerçevesi; iki körükten NavMesh ve oyuncuyla fiziksel geçiş PASS.
  generated/cinema-survival-train-art-pc-acceptance.txt.
- İlk24düşman/yürüyüş testi efekt OFF iken21.65–22.23ms, ON18.03–22.52ms.
  Editor GC182–239KiB/frame. generated/cinema-moving-budget.txt.
- Inspector altı prefab üzerinde kilitliydi. Kilit açılıp seçili Inspector
  boşaltıldıktan sonra aynı1018x588 Ultra GameView'de24gerçek AI/yürüyüş:
  OFF5.05/5.09ms, ON6.07/5.31ms; ON P95=8.54/7.26ms.
  GC15.30–15.59KiB/frame (Editor dahil). PlayerLoop2.89–3.42ms,
  BehaviourUpdate.27–.35ms, Physics.Simulate.056–.069ms;
  InspectorWindow.Render0ms. generated/cinema-cpu-detail.txt.
  Gözlenen ağır Editor kasmasının güçlü nedeni kilitli prefab Inspector'udur;
  bu bir Android oyun optimizasyon sonucu değildir. Efektin kısa PC örneğinde
  maliyeti kabaca.2–1ms; GPU ve termal uzun oturum ayrıca ölçülecek.
- Draw Calls Count geçerli görünüp sürekli0 döndürdü: bu sayaç kullanışlı veri
  üretmedi;0draw call veya mesh-batching kazancı olarak yorumlanmaz.
- Ara tanı kaydı yalnız4düşman ve tek örnekte duran markerlarla üretildi;
  onun sayıları kabul kanıtı yapılmadı. Son CPU kaydı WrapAround ve gerçek24
  düşman kullanır. Önceki D48/D51 kabul dosyaları korunur.

## Yeniden kullanım ve sonraki kapı

D53 güncel ışık denemesi güneşi ve bu sayfadaki world-Z koyu bandı kapatır.
Tek gerçek oyuncu feneri ve gölgesi eklenmiştir; bu sayfanın önceki "gölgesiz"
tanımı D52'nin tarihsel durumudur. Güncel ayarlar/kanıt LOCAL_LIGHTING.md'de.

Wait Your Turn → Survival → Install Maquette Cinema (Play durmuş olmalı).
D51 kurulumunu tekrar yapmak ışık/malzemeleri sıfırlayabilir; ardından bu
kurulumu yeniden uygulayın. Gameplay fingerprint uyuşmazsa sahne kaydedilmez.
Render Maquette Preview ardından cinema önizlemesi için installer Preview
kullanılır. Check Moving Cinema Budget / Check Cinema CPU Detail yalnız
Survival Play'de, normal kayıt dosyasından ayrı test mağazasında çalışır;
ikisi aynı anda çalıştırılmaz. Editor testi yaklaşık20s, uzun oturum değildir.

Şimdi kullanıcı görünüm değerlendirmesi; ardından karakter/zombi maket
modelleri mevcut ölçek/collider/muzzle sözleşmesine göre. İlk giydirme sonrası
A54'te kalabalık sahne GPU/FPS/ısı kabulü yapılmadan bütün modeli çoğaltmayın.
CTR, oynanış keyfi veya gelir bu görsel denemeden kanıtlanamaz.
