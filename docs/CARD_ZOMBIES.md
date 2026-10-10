# Karton zombiler ve dönüş — D54, 2026-10-10

Survival sahnesinin düşman şablonu artık özgün, katlanmış karton insan modellerini kullanır. Intro/Normal, Fast ve Tough mevcut profil, ödül, sağlık, hız ve saldırı değerlerini korur. Boss görünümü kaynakta hazırdır; boss dalgası, özel saldırı veya büyük fizik gövdesi henüz tanımlanmadı. Battle sahnesine bu sanat kurulmadı; görsel bileşen ortak Enemies assembly'sindedir, aynı şablon bağlantısıyla kullanılabilir.

| Model | Üçgen | Oyun görsel ölçeği | Yaklaşık boy |
|---|---:|---:|---:|
| Normal / Intro | 1.536 | 1 | 1,745 m |
| Fast | 1.536 | 0,84 | 1,466 m |
| Tough | 1.680 | 1,13 | 1,972 m |
| Boss taslağı | 1.768 | 1,45 | 2,530 m |

Her düşmanda tek SkinnedMeshRenderer, tek paylaşılan karton malzeme, 15 kemik ve vertex başına bir kemik ağırlığı vardır. Yüz çizimleri ve kat izleri geometri/vertex renktir; büyük doku atlası, ilave ışık veya ek gölge haritası eklenmedi. Yeni malzeme mevcut 64px karton grain'ini paylaşır. Eski kapsül ve yön çubuğu renderer'ları kapalıdır; collider ve NavMeshAgent aynıdır. Actor fill, fener dışında kalan yakın düşmanların da okunması içindir.

## Modelleme sözleşmesi

Unity: 1 birim = 1 metre, Y yukarı, yüz +Z, taban Y=0; yatay kök origin ayaklar arasındadır. Blender dönüşümü `(x, -z, y)`; birleştirilmiş mesh ve deform grupları native `.blend` içinde tutulur. Her kemik rest rotation identity; hips Y=0,80, chest Y=1,05, head Y=1,40. Kollar üçer, bacaklar üçer kemiktir. Kaynaktaki önizleme zemini/ışık/kamera Unity'ye aktarılmaz. Türün ölçeği sadece görsel child'dadır, düşmanın fizik kökü ölçeklenmez.

Kaynak: `ArtSource/CardZombies/CardZombies.blend`; yeniden üretim `Tools/TrainArt/build_card_zombies.py` ile Blender background çalışması. Native kaynakta Idle/Walk/Attack iskelet action'ları bulunur. Unity'de hafif `CardZombieVisual` pozları kullanılır; kaynak action'ları Animator'a aktarılmış klipler değildir. Karton eklemlerin sert, bir kemiğe bağlı deformasyonu bilinçli sanat tercihidir.

## Hareket ve saldırı

NavMeshAgent tek hareket sahibidir. Unity animasyonu dünya kökünün gerçek yatay yer değiştirmesinden adım fazı üretir; durunca yürüyüş azalır, havuzda yeni hayat başlayınca eski poz/faz temizlenir. Teleport büyük adım olarak sayılmaz. Saldırı kolları mevcut MeleeAttack.WindingUp üzerinden hareket eder; vuruş zamanı/hasar görsel animasyona verilmez. Görünmeyen aktörlerde poz hesaplaması atlanır. Ölüm mevcut havuzda hemen döner; ölüm animasyonu için mevcut yaşam/ödül düzeni değiştirilmedi.

Oyuncu ve botları taşıyan ortak PlayerMotor, yürürken 0,20s, hedefe dönerken 0,14s SmoothDampAngle kullanır; üst hız mevcut720°/s'dir. Fener aynı köke bağlı olduğu için yumuşar. AutoAim gerçek görünür hedefe ateş etmeyi sürdürür; görsel dönüş gecikmesi isabet/menzil hesabını değiştirmez. Inspector'dan süreler ayarlanır; yeniden konumlandırma açısal ivmeyi sıfırlar.

Bulanıklık 0,55'ten0,70'e çıkarıldı. Aynı384px sınırı, iki blur pass ve kompozit pass kullanılır; vagon ve yakın saldırganların korunan net bandı değişmedi.

## Dışarıdaki kayma incelemesi

Önceki D53 yoğunluk aracı, Restart'tan1s sonra dış düşmanları elle üretiyordu. O sırada10s yaklaşma devam ediyor ve dekor akıyordu. Bu yapay fixture artık durmuş Defense fazına geçtikten sonra spawn eder. Normal Survival spawner zaten yalnız Defense'da üretir; SurvivalJourney dalga bitip ExteriorAlive=0 olmadan kalkış başlatmaz. Kalkış çözümü binen düşmanları tutar, dışarıdaki aktörleri havuza döndürür. Aktörleri akan dekorun child'ı yapma veya animasyonla yer değiştirme eklenmedi.

Kısa kabul çıktısı `docs/generated/card-zombies-check.txt`, oyun görüntüleri `card-zombies-game.png` ve `card-zombies-boarded.png`. Kurulum receipt'i tüm collider/agent/obstacle ve bunların dünya dönüşümlerinin önce/sonra eşitliğini doğrular. Editor performansı telefon FPS/ısı/pil kabulü değildir; Android'de bu skinning yükü ayrıca ölçülecek.

## Built-in / URP kararı

İkisi de sahnenin ekrana nasıl çizileceğini belirleyen render sistemleridir; oyun modu, fizik veya düşman zekâsı değildir. Built-in mevcut eski sistemdir; URP modern, ayarlanabilir render pipeline'dır ve kullanıcının ağırlıklı URP assetleriyle uyumludur. Unity6.6 belgeleri Built-in'i deprecated olarak işaretler; Unity6.7LTS yaşam döngüsü boyunca hata düzeltme/bakım sürecektir. [Resmi karşılaştırma](https://docs.unity.com/en-us/engine/6000.6/manual/render-pipelines/choose-a-render-pipeline/feature-comparison), [Unity2026 render stratejisi](https://unity.com/topics/render-pipelines-strategy-for-2026).

Öneri: daha fazla Built-in'e özel efekt biriktirmeden, ayrı Git branch/check-out üzerinde URP denemesi. Bu turn'de geçiş yapılmadı. Modeller/rig ve oyun kuralları taşınabilir; özel MaquetteCard shader ve OnRenderImage kamera efekti otomatik malzeme dönüştürücüyle çözülmez. URP shader ve render feature/volume karşılıkları gerekir. Mobilde az ışıklı Forward başlangıcı, aynı görüntü/çözünürlük/24AI senaryosunda PC ve A54 karşılaştırması; ışık, cam/metal, iki mod ve touch UI kabulü sonrasında birleştirme. URP tek başına performans veya güzel görünüm garantisi değildir.

Kabul sonucu:24AI,1060�576 Ultra Editor,610 �rnek karede ortalama6,56ms/P959,02ms. D�rt d��manl�k ger�ek dalga ve havuz s�f�rlama PASS;292 fizik componenti ve eski b�t�n local pozlar Git ba�lang�c�yla ayn�. �l��mden sonra sadece akt�r fill malzemesi y�kseltildi; animasyon/mesh/���k say�s� de�i�medi.
